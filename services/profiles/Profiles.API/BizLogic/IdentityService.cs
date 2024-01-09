using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.BizLogic
{
    /*
        public interface IIdentityService
        {
            Task<Responses<TokenModel>> LoginAsync(LoginDTO login);
            Task<Responses<TokenModel>> RefreshTokenAsync(TokenModel request);


        }
        public class IdentityService : IIdentityService
        {
            private readonly TotalMOCDBContext _context;
            private readonly ServiceConfiguration _appSettings;
            private RefreshTokenRepository _refreshrepo = null;
            private readonly TokenValidationParameters _tokenValidationParameters;
            private readonly ILogger _logger;
            private readonly UserRepository _userrepo;
            public IdentityService(TotalMOCDBContext context,
                IOptions<ServiceConfiguration> settings,
                TokenValidationParameters tokenValidationParameters,
                 ILoggerFactory logFactory)
            {
                _logger = logFactory.CreateLogger<IdentityService>();
                _context = context;
                _appSettings = settings.Value;
                _tokenValidationParameters = tokenValidationParameters;
                _refreshrepo = new RefreshTokenRepository(context, logFactory);
                _userrepo = new UserRepository(context, logFactory);
            }


            public async Task<Responses<TokenModel>> LoginAsync(LoginDTO login)
            {
                Responses<TokenModel> response = new Responses<TokenModel>();
                try
                {

                    UserDTO loginUser = _refreshrepo.Authenticateuser(login.UserName, login.Password);

                    if (loginUser == null)
                    {
                        response.IsSuccess = false;
                        response.Message = "Invalid Username And Password";
                        return response;
                    }

                    AuthenticationResult authenticationResult = await AuthenticateAsync(loginUser);
                    if (authenticationResult != null && authenticationResult.Success)
                    {
                        response.Data = new TokenModel() { Token = authenticationResult.Token, RefreshToken = authenticationResult.RefreshToken };
                    }
                    else
                    {
                        response.Message = "Something went wrong!";
                        response.IsSuccess = false;
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //private List<RolesMaster> GetUserRole(long UserId)
            //{
            //    try
            //    {
            //        List<RolesMaster> rolesMasters = (from UM in _context.UsersMaster
            //                                          join UR in _context.UserRoles on UM.UserId equals UR.UserId
            //                                          join RM in _context.RolesMaster on UR.RoleId equals RM.RoleId
            //                                          where UM.UserId == UserId
            //                                          select RM).ToList();
            //        return rolesMasters;
            //    }
            //    catch (Exception)
            //    {
            //        return new List<RolesMaster>();
            //    }
            //}

            public async Task<AuthenticationResult> AuthenticateAsync(UserDTO user)
            {
                // authentication successful so generate jwt token  
                AuthenticationResult authenticationResult = new AuthenticationResult();
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var key = Encoding.ASCII.GetBytes(_appSettings.JwtSettings.Secret);

                    ClaimsIdentity Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("UserName",user.UserName==null?"":user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        });
                    //TODO
                    //foreach (var item in GetUserRole(user.UserId))
                    //{
                    //    Subject.AddClaim(new Claim(ClaimTypes.Role, item.RoleName));
                    //}

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = Subject,
                        Expires = DateTime.UtcNow.Add(_appSettings.JwtSettings.TokenLifetime),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    authenticationResult.Token = tokenHandler.WriteToken(token);
                    var refreshToken = new RefreshToken
                    {
                        Token = Guid.NewGuid().ToString(),
                        JwtId = token.Id,
                        UserId = user.UserId,
                        CreationDate = DateTime.UtcNow,
                        ExpiryDate = DateTime.UtcNow.AddMonths(6)
                    };
                    _refreshrepo.AddEditRefreshToken(refreshToken);

                    authenticationResult.RefreshToken = refreshToken.Token;
                    authenticationResult.Success = true;
                    return authenticationResult;
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            public async Task<Responses<TokenModel>> RefreshTokenAsync(TokenModel request)
            {
                Responses<TokenModel> response = new Responses<TokenModel>();
                try
                {
                    var authResponse = await GetRefreshTokenAsync(request.Token, request.RefreshToken);
                    if (!authResponse.Success)
                    {

                        response.IsSuccess = false;
                        response.Message = string.Join(",", authResponse.Errors);
                        return response;
                    }
                    TokenModel refreshTokenModel = new TokenModel();
                    refreshTokenModel.Token = authResponse.Token;
                    refreshTokenModel.RefreshToken = authResponse.RefreshToken;
                    response.Data = refreshTokenModel;
                    return response;
                }
                catch (Exception ex)
                {


                    response.IsSuccess = false;
                    response.Message = "Something went wrong!";
                    return response;
                }
            }

            private async Task<AuthenticationResult> GetRefreshTokenAsync(string token, string refreshToken)
            {
                var validatedToken = GetPrincipalFromToken(token);

                if (validatedToken == null)
                {
                    return new AuthenticationResult { Errors = new[] { "Invalid Token" } };
                }

                var expiryDateUnix =
                    long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(expiryDateUnix);

                if (expiryDateTimeUtc > DateTime.UtcNow)
                {
                    return new AuthenticationResult { Errors = new[] { "This token hasn't expired yet" } };
                }

                var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                var storedRefreshToken = _refreshrepo.GetRefreshTokenByToken(refreshToken);

                if (storedRefreshToken == null)
                {
                    return new AuthenticationResult { Errors = new[] { "This refresh token does not exist" } };
                }

                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthenticationResult { Errors = new[] { "This refresh token has expired" } };
                }

                if (storedRefreshToken.Used.HasValue && storedRefreshToken.Used == true)
                {
                    return new AuthenticationResult { Errors = new[] { "This refresh token has been used" } };
                }

                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthenticationResult { Errors = new[] { "This refresh token does not match this JWT" } };
                }

                storedRefreshToken.Used = true;
                _refreshrepo.AddEditRefreshToken(storedRefreshToken);
                await _context.SaveChangesAsync();
                string strUserId = validatedToken.Claims.Single(x => x.Type == "UserId").Value;
                int userId = 0;
                int.TryParse(strUserId, out userId);
                var user = _userrepo.GetUserById(userId);
                if (user == null)
                {
                    return new AuthenticationResult { Errors = new[] { "User Not Found" } };
                }

                return await AuthenticateAsync(user);
            }

            private ClaimsPrincipal GetPrincipalFromToken(string token)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var tokenValidationParameters = _tokenValidationParameters.Clone();
                    tokenValidationParameters.ValidateLifetime = false;
                    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                    if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                    {
                        return null;
                    }

                    return principal;
                }
                catch
                {
                    return null;
                }
            }

            private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
            {
                return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                       jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                           StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
    */
}

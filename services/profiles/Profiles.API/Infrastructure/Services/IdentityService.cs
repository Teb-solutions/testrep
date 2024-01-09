using EasyGas.Security;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Models;
using Profiles.API.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;
using Profiles.API.Services;
using Profiles.API.ViewModels;
using System.Text.RegularExpressions;
using EasyGas.Shared.Models;
using System.Security.Claims;

namespace Profiles.API.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ApiSettings _appSettings;
        private IHttpContextAccessor _context;
        private readonly ProfilesDbContext _db;
        private IJWTUtils _jwtUtils;
        private ICartService _cartService;
        private ICrmApiService _crmApiService;
        private ILogger _logger;

        public IdentityService(ProfilesDbContext db, IOptions<ApiSettings> appSettings, IHttpContextAccessor context,
            IJWTUtils jWTUtils, ICartService cartService, ICrmApiService crmApiService, ILogger<IdentityService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _db = db;
            _jwtUtils = jWTUtils;
            _appSettings = appSettings.Value;
            _cartService = cartService;
            _crmApiService = crmApiService;
            _logger = logger;
        }

        public int GetUserIdentity()
        {
            string idString = _context.HttpContext.User.FindFirst("sub").Value;
            return int.Parse(idString);
        }

        public string GetUserName()
        {
            return _context.HttpContext.User.Identity.Name;
        }

        public int GetBusinessEntityId()
        {
            string businessEntityId = _context.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            if (string.IsNullOrEmpty(businessEntityId))
            {
                return 0;
            }

            return int.Parse(businessEntityId);
        }

        public async Task<CommandResult> Authenticate(LoginModel model, string ipAddress, string tempUserToken = null)
        {
            User? user = null;
            if (model.UserType == null)
            {
                List<UserType> userTypes = new List<UserType>() { UserType.RELAY_POINT, UserType.DISTRIBUTOR, UserType.DEALER,
                    UserType.ALDS_ADMIN, UserType.MARSHAL, UserType.SECURITY };
                user = _db.Users.
                    Include(u => u.Profile).
                    Include(u => u.RefreshTokens).
                    Include(u => u.Roles).ThenInclude(p => p.Role).
                    //Include(u => u.Logins).
                    Include(u => u.BusinessEntity).
                    Include(u => u.Branch).
                    SingleOrDefault(p => p.UserName == model.UserName && userTypes.Contains(p.Type));
            }
            else
            {
                user = _db.Users.
                    Include(u => u.Profile).
                    Include(u => u.RefreshTokens).
                    Include(u => u.Roles).ThenInclude(p => p.Role).
                    //Include(u => u.Logins).
                    Include(u => u.BusinessEntity).
                    Include(u => u.Branch).
                    SingleOrDefault(p => p.UserName == model.UserName && p.Type == model.UserType);
            }

            // validate
            if (user == null)
            {
                return CommandResult.FromValidationErrors("Username/password is invalid");
            }

            bool credentialsValid = false;
            if (model.GrantType == LoginModel.PasswordGrantType)
            {
                string hashPass = Helper.HashPassword(model.Credentials);
                if (hashPass == user.Password)
                {
                    credentialsValid = true;
                }
            }
            else if (model.GrantType == LoginModel.OtpGrantType)
            {
                credentialsValid = true; // as OTP value is already validated
            }

            if (!credentialsValid)
            {
                return CommandResult.FromValidationErrors("Username/password is invalid");
            }

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;
            _db.RefreshTokens.Add(refreshToken);

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // add login
            var userLastLogin = _db.UserLogin.Where(p => p.UserId == user.Id).OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt;
            user.LastLogin = userLastLogin;
            if (user.Logins == null)
            {
                user.Logins = new List<UserLogin>();
            }
            user.Logins.Add(new UserLogin(model.Source, model.DeviceId, ipAddress));

            user.Profile.DeviceId = model.DeviceId;

            // save changes to db
            _db.Update(user);
            await _db.SaveChangesAsync();

            if (user.Type == UserType.ADMIN)
            {
                var response = new AdminGrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    CityName = user.Branch?.Name,
                    CityLat = user.Branch?.Lat,
                    CityLng = user.Branch?.Lng,
                    UserType = user.Type,
                    LastLogin = user.LastLogin,
                    Roles = user.Roles.Select(p => p.Role.Name).ToList()
                };

                return new CommandResult(HttpStatusCode.OK, response);
            }
            else if (user.Type == UserType.CUSTOMER)
            {
                var response = new CustomerGrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    UserType = user.Type,
                };

                if (!string.IsNullOrEmpty(tempUserToken))
                {
                    await _cartService.ConvertTempCart(tempUserToken, user.Id.ToString());
                }

                return new CommandResult(HttpStatusCode.OK, response);
            }
            else if (user.Type == UserType.DRIVER)
            {
                var vehicle = _db.Vehicles.Where(p => p.DriverId == user.Id && p.IsActive == true).FirstOrDefault();

                var response = new DriverGrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    UserType = user.Type,
                    VehicleId = vehicle != null ? vehicle.Id : null,
                    VehicleRegNo = vehicle != null ? vehicle.RegNo : "N/A"
                };

                return new CommandResult(HttpStatusCode.OK, response);
            }
            else if (user.Type == UserType.RELAY_POINT || user.Type == UserType.DISTRIBUTOR || user.Type == UserType.DEALER || user.Type == UserType.ALDS_ADMIN)
            {
                var response = new BusinessEntityGrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    UserType = user.Type,
                    BusinessEntityId = user.BusinessEntityId,
                    BusinessEntityName = user.BusinessEntity.Name
                };

                return new CommandResult(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new GrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    UserType = user.Type
                };

                if (user.Type == UserType.CUSTOMER_CARE)
                {
                    await _crmApiService.UpdateStaffDeviceId(user.Id, model.DeviceId, response.AccessToken);
                }

                return new CommandResult(HttpStatusCode.OK, response);
            }
            
        }

        public async Task<CommandResult> RefreshToken(string token, string ipAddress)
        {
            if (string.IsNullOrEmpty(token))
            {
                return CommandResult.FromValidationErrors(new string[] { "Invalid token "});
            }
            
            var user = getUserByRefreshToken(token);
            if (user == null)
            {
                return CommandResult.FromValidationErrors(new string[] { "Invalid token " });
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null)
            {
                return CommandResult.FromValidationErrors(new string[] { "Invalid token " });
            }

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _db.Update(user);
                await _db.SaveChangesAsync();
            }

            if (!refreshToken.IsActive)
                return CommandResult.FromValidationErrors("Invalid Credentials");

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
            newRefreshToken.UserId = user.Id;
            user.RefreshTokens.Add(newRefreshToken);

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // save changes to db
            _db.Update(user);
            await _db.SaveChangesAsync();

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            if (user.Type == UserType.ADMIN)
            {
                var response = new AdminGrantAccessResult()
                {
                    Name = user.Profile.FirstName,
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = newRefreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    CityName = user.Branch?.Name,
                    CityLat = user.Branch?.Lat,
                    CityLng = user.Branch?.Lng,
                    UserType = user.Type,
                    LastLogin = user.LastLogin,
                    Roles = user.Roles.Select(p => p.Role.Name).ToList()
                };

                return new CommandResult(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new GrantAccessResult()
                {
                    Name = user.Profile.GetFullName(),
                    Mobile = user.Profile.Mobile,
                    AccessToken = jwtToken,
                    RefreshToken = newRefreshToken.Token,
                    TenantId = user.TenantId,
                    CityId = user.BranchId,
                    UserType = user.Type,
                };

                return new CommandResult(HttpStatusCode.OK, response);
            }
        }

        public async Task<CommandResult> Register(RegisterModel registerModel)
        {
            if (registerModel == null)
            {
                return CommandResult.FromValidationErrors("Invalid data ");
            }

            var existing = _db.Users.Any(x => x.UserName == registerModel.UserName && x.Type == registerModel.Type);
            if (existing)
            {
                return CommandResult.FromValidationErrors($"Username ({registerModel.UserName}) already exists.");
            }

            if (!string.IsNullOrEmpty(registerModel.Code))
            {
                var existingCode = _db.Profiles.Any(x => x.Code == registerModel.Code);
                if (existingCode)
                {
                    return CommandResult.FromValidationErrors($"Code ({registerModel.Code}) already exists.");
                }
            }

            var tenant = _db.Tenants.Where(p => p.Id == registerModel.TenantId).FirstOrDefault();
            if (tenant == null)
            {
                return CommandResult.FromValidationErrors($"Tenant is invalid.");
            }

            if (registerModel.BranchId > 0)
            {
                var branch = _db.Branches.Where(p => p.Id == registerModel.BranchId).FirstOrDefault();
                if (branch == null)
                {
                    return CommandResult.FromValidationErrors($"Branch is invalid.");
                }
            }

            if (registerModel.BusinessEntityId > 0)
            {
                var businessEntity = _db.BusinessEntities.Where(p => p.Id == registerModel.BusinessEntityId).FirstOrDefault();
                if (businessEntity == null)
                {
                    return CommandResult.FromValidationErrors($"Business Entity is invalid.");
                }
            }

            var role = _db.Roles.Where(p => p.Name == registerModel.RoleName).FirstOrDefault();
            if (role == null)
            {
                return CommandResult.FromValidationErrors($"Role is not added.");
            }

            User user = new User()
            {
                BranchId = registerModel.BranchId,
                TenantId = registerModel.TenantId,
                BusinessEntityId = registerModel.BusinessEntityId,
                CreationType = registerModel.CreationType,
                OtpValidated = true,
                UserName = registerModel.UserName,
                Type = registerModel.Type,
                Profile = new UserProfile()
                {
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    AgreedTerms = true,
                    Code = registerModel.Code,
                    Email = registerModel.Email,
                    Mobile = registerModel.Mobile,
                    Source = registerModel.Source
                }
            };
            user.Roles.Add(new UserRole(role.Id));

            if (!string.IsNullOrEmpty(registerModel.Password))
            {
                var passwordPolicyErrors = ValidatePasswordPolicy(user.Type, registerModel.Password);
                if (passwordPolicyErrors.Count() > 0)
                {
                    return CommandResult.FromValidationErrors(passwordPolicyErrors);
                }

                user.Password = Helper.HashPassword(registerModel.Password);
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Register New User Created | userType: " + registerModel.Type + "username: " + registerModel.UserName);

            return new CommandResult(HttpStatusCode.OK, new CreateProfileResponse{ UserId = user.Id });
        }

        public async Task<CommandResult> ChangePassword(ChangePasswordModel request,int userId)
        {
            var user = _db.Users.SingleOrDefault(p => p.Id == userId);
            if (user == null)
            {
                return CommandResult.FromValidationErrors("No user found");
            }
            if (user.Password != Helper.HashPassword(request.CurrentPassword))
            {
                return CommandResult.FromValidationErrors("Current password is invalid");
            }

            var passwordPolicyErrors = ValidatePasswordPolicy(user.Type, request.NewPassword);
            if (passwordPolicyErrors.Count() > 0)
            {
                return CommandResult.FromValidationErrors(passwordPolicyErrors);
            }

            user.Password = Helper.HashPassword(request.NewPassword);
            _db.Update(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Password changed for user: " + userId);
            return new CommandResult(HttpStatusCode.OK, new ApiResponse("Password successfully updated"));
        }

        public IEnumerable<string> ValidatePasswordPolicy(UserType type, string newPassword)
        {
            if (type == UserType.ADMIN || type == UserType.CUSTOMER_CARE)
            {
                if (newPassword.Length < 15)
                {
                    yield return "New Password should have min 15 characters";
                }
                if (newPassword.Length > 18)
                {
                    yield return "New Password should have at most 18 characters";
                }
                if (!Regex.Match(newPassword, @"[A-Z]", RegexOptions.Singleline).Success)
                {
                    yield return "New Password should have alteast one UPPERASE (A-Z)";
                }
                if (!Regex.Match(newPassword, @"[a-z]", RegexOptions.ECMAScript).Success)
                {
                    yield return "New Password should have alteast one lowercase (a-z)";
                }
                if (!Regex.Match(newPassword, @"[0-9]", RegexOptions.ECMAScript).Success)
                {
                    yield return "New Password should have alteast one number (0-9)";
                }
                if (!Regex.Match(newPassword, @"[!,@,#,$,%,&,*]", RegexOptions.ECMAScript).Success)
                {
                    yield return "New Password should have alteast one special character (!@#$%&*)";
                }
            }
            else
            {
                if (newPassword.Length < 8)
                {
                    yield return "New Password should have min 8 characters";
                }
                if (newPassword.Length > 18)
                {
                    yield return "New Password should have at most 18 characters";
                }
            }
        }

        private User getUserByRefreshToken(string token)
            {
                var user = _db.Users
                .Include(p => p.Profile)
                .Include(p => p.RefreshTokens)
                .Include(p => p.Roles)
                .ThenInclude(p => p.Role)
                .SingleOrDefault(u => u.RefreshTokens
                .Any(t => t.Token == token));
                return user;
            }

            private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
            {
                var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
                revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
                return newRefreshToken;
            }

            private void removeOldRefreshTokens(User user)
            {
                // remove old inactive refresh tokens from user based on TTL in app settings
                user.RefreshTokens.RemoveAll(x =>
                    !x.IsActive &&
                    x.Expires.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
            }

            private void revokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
            {
                // recursively traverse the refresh token chain and ensure all descendants are revoked
                if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
                {
                    var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                    if (childToken.IsActive)
                        revokeRefreshToken(childToken, ipAddress, reason);
                    else
                        revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
                }
            }

            private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
                token.ReplacedByToken = replacedByToken;
            }
        }
}

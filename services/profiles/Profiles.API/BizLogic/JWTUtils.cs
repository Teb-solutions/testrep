using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Jose;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace EasyGas.Security
{
    public interface IJWTUtils
    {
        string GenerateJwtToken(User user);
        //int? ValidateJwtToken(string token);
        RefreshToken GenerateRefreshToken(string ipAddress = "");
    }

    public class JWTUtils : IJWTUtils
    {

        private readonly ApiSettings _appSettings;

        public JWTUtils(IOptions<ApiSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.JwtTokenPrivateKey);

            var fullName = user.Profile.GetFullName();
            fullName = string.IsNullOrEmpty(fullName) ? user.UserName : fullName;

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, fullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.PrimaryGroupSid, _appSettings.TenantUidDefault),
            };

            if (user.BusinessEntityId != null)
            {
                claims.Add(new Claim(ClaimTypes.Sid, user.BusinessEntityId.ToString()));
            }

            foreach(var userRole in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var tokenExpiryMin = _appSettings.JwtTokenExpiryMin;

            
            //TODO remove aftr testing
            //if (user.Id == 6 && user.Type == Shared.Enums.UserType.DRIVER)
            //{
             //   tokenExpiryMin = 5;
            //}

            /*
             * TODO remove aftr testing
            if (user.Type == Shared.Enums.UserType.DRIVER || user.Type == Shared.Enums.UserType.RELAY_POINT)
            {
                tokenExpiryMin = 2;
            }
            */

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpiryMin),
                Issuer = _appSettings.JwtTokenIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.RefreshTokenExpiryMin),
                CreatedByIp = ipAddress,
                CreatedAt = DateMgr.GetCurrentIndiaTime(),
                UpdatedAt = DateMgr.GetCurrentIndiaTime()
            };

            return refreshToken;
        }

        /*
        public string DecodeToken(string token, byte[] privateKey)
        {
            var decoded = JWT.Decode(token, privateKey, JwsAlgorithm.HS256);
            return decoded;
        }
        */
    }
}
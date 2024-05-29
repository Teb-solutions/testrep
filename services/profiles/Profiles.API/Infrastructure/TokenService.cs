using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Profiles.API.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Profiles.API.Infrastructure
{
    public class TokenService : ITokenService
    {
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly IDistributedCache _cache;
        private readonly IProfileQueries _profileQueries;

        public TokenService(IOptions<ApiSettings> apiSettings, IDistributedCache cache, IProfileQueries profileQueries)
        {
            _apiSettings = apiSettings;
            _cache = cache;
            _profileQueries = profileQueries;
        }

        public async Task<BackendUserProfileModel> GetUserDetailsAsync(JwtSecurityToken cognitoJwtToken)
        {
            /*
            if (_cache.TryGetValue(username, out List<string> roles))
            {
                return roles;
            }
            */
            var cognitoUsername = cognitoJwtToken.Claims.Where(p => p.Type == "username").FirstOrDefault().Value;
            var userDetails = await _profileQueries.GetUserByCognitoUsername(cognitoUsername);
            if (userDetails != null)
            {
                // add to cache
            }
            
            return userDetails;
        }

        public string GenerateJwtToken(BackendUserProfileModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_apiSettings.Value.JwtTokenPrivateKey);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.PrimaryGroupSid, user.TenantId.ToString()),
                new Claim("username", user.CognitoUsername),
                //new Claim(JwtRegisteredClaimNames.Iss, "Easygas"),
            };

            /*
            if (user.BusinessEntityId != null)
            {
                claims.Add(new Claim(ClaimTypes.Sid, user.BusinessEntityId.ToString()));
            }
            */

            foreach (var userRole in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.UtcNow.AddMinutes(tokenExpiryMin),
                Issuer = _apiSettings.Value.JwtTokenIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Mvc;

namespace Profiles.API.Infrastructure
{
    public interface ITokenService
    {
        string GenerateJwtToken(BackendUserProfileModel user);
        Task<BackendUserProfileModel> GetUserDetailsAsync(JwtSecurityToken cognitoJwtToken);
    }
}

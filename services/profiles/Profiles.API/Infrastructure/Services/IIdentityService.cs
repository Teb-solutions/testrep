using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Infrastructure.Services
{
    public interface IIdentityService
    {
        int GetUserIdentity();
        string GetUserName();
        int GetBusinessEntityId();

        Task<CommandResult> Register(RegisterModel registerModel);
        Task<CommandResult> Authenticate(LoginModel model, string ipAddress, string tempUserToken = null);
        Task<CommandResult> RefreshToken(string token, string ipAddress);
        Task<CommandResult> ChangePassword(ChangePasswordModel request, int userId);

        IEnumerable<string> ValidatePasswordPolicy(UserType type, string newPassword);
    }
}

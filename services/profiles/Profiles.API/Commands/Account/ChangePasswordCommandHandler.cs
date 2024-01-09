using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Models;
using System.Text.RegularExpressions;

namespace EasyGas.Services.Profiles.Commands
{
    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
    {
        private readonly ProfilesDbContext _db;
        public ChangePasswordCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(ChangePasswordCommand command)
        {
            var user = _db.Users.SingleOrDefault(p => p.Id == command._userId);
            if (user == null)
            {
                return CommandHandlerResult.Error("No user found");
            }
            if (user.Password != Helper.HashPassword(command._changePasswordModel.CurrentPassword))
            {
                return CommandHandlerResult.Error("Current password is invalid");
            }

            //password policy for admin
            if (user.Type == Shared.Enums.UserType.ADMIN)
            {
                if (command._changePasswordModel.NewPassword.Length < 15)
                {
                    return CommandHandlerResult.Error("New Password should have min 15 characters");
                }
                if (command._changePasswordModel.NewPassword.Length > 18)
                {
                    return CommandHandlerResult.Error("New Password should have at most 18 characters");
                }
                if (!Regex.Match(command._changePasswordModel.NewPassword, @"[A-Z]", RegexOptions.Singleline).Success)
                {
                    return CommandHandlerResult.Error("New Password should have alteast one UPPERASE (A-Z)");
                }
                if (!Regex.Match(command._changePasswordModel.NewPassword, @"[a-z]", RegexOptions.ECMAScript).Success)
                {
                    return CommandHandlerResult.Error("New Password should have alteast one lowercase (a-z)");
                }
                if (!Regex.Match(command._changePasswordModel.NewPassword, @"[0-9]", RegexOptions.ECMAScript).Success)
                {
                    return CommandHandlerResult.Error("New Password should have alteast one number (0-9)");
                }
                if (!Regex.Match(command._changePasswordModel.NewPassword, @"[!,@,#,$,%,&,*]", RegexOptions.ECMAScript).Success)
                {
                    return CommandHandlerResult.Error("New Password should have alteast one special character (!@#$%&*)");
                }
            }

            user.Password = Helper.HashPassword(command._changePasswordModel.NewPassword);

            return CommandHandlerResult.OkDelayed(this,
                x => new ApiResponse( "Password successfully updated"));
        }
    }
}

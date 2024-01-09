using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class ChangePasswordCommand : CommandBase
    {
        public ChangePasswordModel _changePasswordModel { get; }
        public int _userId { get; }

        public ChangePasswordCommand(ChangePasswordModel changePasswordModel, int userId)
        {
            _changePasswordModel = changePasswordModel;
            _userId = userId;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (string.IsNullOrEmpty(_changePasswordModel.CurrentPassword))
            {
                yield return "Current password is empty";
            }
            if (string.IsNullOrEmpty(_changePasswordModel.NewPassword))
            {
                yield return "New password is empty";
            }
            if (_userId <= 0)
            {
                yield return "User is invalid";
            }
        }
    }
}

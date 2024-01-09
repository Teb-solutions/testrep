using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateMobileCommand : CommandBase
    {
        public UserAndProfileModel Profile { get; }
        public UpdateMobileCommand(UserAndProfileModel userAndProfile)
        {
            Profile = userAndProfile;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Profile.UserId <= 0)
            {
                yield return "User is invalid";
            }
            if (string.IsNullOrEmpty(Profile.Mobile))
            {
                yield return "Mobile is invalid";
            }
        }
    }
}

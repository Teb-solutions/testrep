using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddReferralCommand : CommandBase
    {
        private readonly UserAndProfileModel _model;
        public UserAndProfileModel userAndProfileModel { get; }
        public AddReferralCommand(UserAndProfileModel model)
        {
            _model = model;
            userAndProfileModel = model;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (_model == null)
            {
                yield return "Invalid or no payload recieved";
            }
            if (_model.UserId <= 0)
            {
                yield return "User is invalid";
            }
            if (String.IsNullOrEmpty(_model.ReferralCode))
            {
                yield return "Referral Code is empty";
            }
        }
    }
}

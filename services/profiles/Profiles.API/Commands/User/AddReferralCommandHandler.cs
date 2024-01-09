using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddReferralCommandHandler : ICommandHandler<AddReferralCommand>
    {
        private readonly ProfilesDbContext _db;
        private AddReferralCommand _command;
        public AddReferralCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }

        public CommandHandlerResult Handle(AddReferralCommand command)
        {
            _command = command;
            var userProfile = _db.Profiles.Where(x => x.UserId == command.userAndProfileModel.UserId).FirstOrDefault();
            if (userProfile == null)
            {
                return CommandHandlerResult.Error($"User does not exist.");
            }
          
            if (!string.IsNullOrEmpty(_command.userAndProfileModel.ReferralCode) && string.IsNullOrEmpty(userProfile.ReferralCode))
            {
                var referredUserProfile = _db.Profiles.Where(x => x.MyReferralCode == _command.userAndProfileModel.ReferralCode).FirstOrDefault();
                if (referredUserProfile == null)
                {
                    return CommandHandlerResult.Error($"Referral Code ({_command.userAndProfileModel.ReferralCode}) does not exist.");
                }
                userProfile.ReferredByUserId = referredUserProfile.UserId;
            }

            // generate referral code
            /*
            string myReferralCode = _voucherMgr.GenerateReferralCode();
            if (string.IsNullOrEmpty(myReferralCode))
            {
                myReferralCode = command.User.Profile.Mobile;
            }
            command.User.Profile.MyReferralCode = myReferralCode;
            */

            //_db.Entry(userProfile).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            return CommandHandlerResult.Ok;
        }
    }
}

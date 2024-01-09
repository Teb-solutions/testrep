using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateProfileCommand : CommandBase
    {
        public bool IsPatch { get; }
        public UserProfile Profile { get; }
        public User User { get; }
        public UpdateProfileCommand(UserAndProfileModel userAndProfile, bool isPatch)
        {
            Profile = GetProfile(userAndProfile);
            User = GetUser(userAndProfile);
            IsPatch = isPatch;
        }

        public UserProfile GetProfile(UserAndProfileModel model)
        {
            UserProfile profile = new UserProfile
            {
                UserId = model.UserId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Mobile = model.Mobile,
                BirthDate = model.BirthDate,
                Email = model.Email,
                Gender = model.Gender,
                GSTN = model.GSTN,
                PAN = model.PAN,
                Code = model.Code,
                ReferralCode = model.ReferralCode,
                AgreedTerms = model.AgreedTerms,
                DeviceId = model.DeviceId,
                OffDay = model.OffDay
            };
            return profile;
        }

        private User GetUser(UserAndProfileModel model)
        {
            var user = new User()
            {
                Password = model.Password,
                UserName = model.UserName,
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                CreationType = model.CreationType,
                Type = model.Type,
                OtpValidated = model.OtpValidated,
                OtpValidatedBy = model.OtpValidatedBy,
            };
            return user;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Profile.UserId == 0)
            {
                yield return "User Id is missing";
            }

            if (!IsPatch)
            {
                foreach (var msg in FullValidation())
                {
                    yield return msg;
                }
            }
        }

        private IEnumerable<string> FullValidation()
        {
            if (string.IsNullOrEmpty(Profile.Mobile))
            {
                yield return "Mobile is missing";
            }
        }

    }
}

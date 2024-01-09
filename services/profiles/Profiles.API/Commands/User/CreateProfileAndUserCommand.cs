using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateProfileAndUserCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public bool ValidateOnly { get; }
        public bool OtpValidated { get; }

        private readonly UserAndProfileModel _model;

        public CreateProfileAndUserCommand(UserAndProfileModel model, bool validateOnly = false, bool otpValidated = true)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model);
            ValidateOnly = validateOnly;
            OtpValidated = otpValidated;

            Profile.User = User;
            User.Profile = Profile;
        }

        private User CreateUserFromModel(UserAndProfileModel model)
        {
            var user = new User()
            {
                Password = model.Password,
                UserName = model.UserName,
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                CreationType = model.CreationType,
                Type = model.Type,
                BusinessEntityId = model.BusinessEntityId > 0 ? model.BusinessEntityId : null,
                OtpValidated = OtpValidated,
                CreatedBy = model.UpdatedBy,
                UpdatedBy = model.UpdatedBy,
            };

            if (OtpValidated)
            {
                user.OtpValidatedAt = DateMgr.GetCurrentIndiaTime();
            }

            if (user.BusinessEntityId != null)
            {
                user.BusinessEntityAttachedAt = DateMgr.GetCurrentIndiaTime();
                user.BusinessEntityAttachedByUserId = int.Parse(model.UpdatedBy);
            }

            return user;
        }

        private UserProfile CreateProfileFromModel(UserAndProfileModel model)
        {
            var profile = new UserProfile()
            {
                BirthDate = model.BirthDate,
                Email = model.Email,
                Mobile = model.Mobile,
                FirstName = model.FirstName,
                Gender = model.Gender,
                LastName = model.LastName,
                OffDay = model.OffDay,
                Source = model.Source,
                GSTN = model.GSTN,
                PAN = model.PAN,
                Code = model.Code,
                ReferralCode = model.ReferralCode,
                DeviceId = model.DeviceId,
                AgreedTerms = model.AgreedTerms,
                Rating = 5,
                SendNotifications = true,
                RegisteredFromLat = model.RegisteredFromLat,
                RegisteredFromLng = model.RegisteredFromLng,
                CreatedBy = model.UpdatedBy
            };

            return profile;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (_model == null)
            {
                yield return "invalid or no payload received";
            }
            if (_model.UserName == null)
            {
                yield return "Username is empty";
            }
            /*
            if (_model.Password== null)
            {
                yield return "Password is empty";
            }
            if (_model.FirstName == null)
            {
                yield return "First Name is empty";
            }
            */
            if (_model.TenantId <= 0)
            {
                yield return "Tenant is invalid";
            }
        }
    }
}

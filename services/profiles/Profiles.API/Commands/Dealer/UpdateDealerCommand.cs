using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Distributor;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateDealerCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public BusinessEntity BusinessEntity { get; }

        public bool ValidateOnly { get; }

        private readonly CreateDealerRequest _model;

        public UpdateDealerCommand(CreateDealerRequest model, bool validateOnly = false)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model);
            BusinessEntity = CreateBusinessEntityFromModel(model);

            ValidateOnly = validateOnly;
        }

        private BusinessEntity CreateBusinessEntityFromModel(CreateDealerRequest model)
        {
            var businessEntity = new BusinessEntity()
            {
                Id = (int)model.Id,
                Code = model.Code,
                Name = model.Name,
                MobileNumber = model.MobileNumber,
                Email = model.Email,

                GSTN = model.GSTN,
                PAN = model.PAN,
                PaymentNumber = model.PaymentNumber,

                Location = model.Location,
                Lat = model.Lat,
                Lng = model.Lng,
                Landmark = model.Landmark,
                Details = model.Details,
                PinCode = model.PinCode,
                State = model.State,
                ParentBusinessEntityId = model.ParentBusinessEntityId,
                //Rating = model.Rating,
                IsActive = true,
                Timings = new List<BusinessEntityTiming>()
            };

            return businessEntity;
        }

        private User CreateUserFromModel(CreateDealerRequest model)
        {
            var user = new User()
            {
                Password = model.AdminUserName,
                UserName = model.AdminUserName,
                CreationType = CreationType.USER,
                OtpValidated = false,
            };
            return user;
        }

        private UserProfile CreateProfileFromModel(CreateDealerRequest model)
        {
            var profile = new UserProfile()
            {
                Email = model.AdminEmail,
                Mobile = model.AdminMobile,
                FirstName = model.Name,
                //LastName = model.AdminLastName,
                Gender = Gender.NotSpecified,
                Source = model.Source,
                Code = model.Code,
                AgreedTerms = true,
                MyReferralCode = model.Code
            };

            return profile;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (_model == null)
            {
                yield return "invalid or no payload received";
            }

            if (_model.Id == null)
            {
                yield return "Dealer not found";
            }
            if (_model.Name == null)
            {
                yield return "Name is empty";
            }
            if (_model.MobileNumber == null)
            {
                yield return "Mobile Number is empty";
            }
        }
    }
}

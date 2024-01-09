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
using Profiles.API.ViewModels.BusinessEntity;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateBusinessEntityCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public BusinessEntity BusinessEntity { get; }

        public bool ValidateOnly { get; }
        public bool UpdateAdminUser { get; }

        private readonly CreateBusinessEntityRequest _model;

        public UpdateBusinessEntityCommand(CreateBusinessEntityRequest model, bool validateOnly = false, bool updateAdminUser = true)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model);
            BusinessEntity = CreateBusinessEntityFromModel(model);

            ValidateOnly = validateOnly;
            UpdateAdminUser = updateAdminUser;
        }

        private BusinessEntity CreateBusinessEntityFromModel(CreateBusinessEntityRequest model)
        {
            var businessEntity = new BusinessEntity()
            {
                Id = (int)model.Id,
                Type = model.Type,
                BranchId = model.BranchId,
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
                UpdatedBy = model.UpdatedBy.ToString(),
                Timings = new List<BusinessEntityTiming>()
            };

            foreach (var workingDay in model.WorkingDaysList)
            {
                businessEntity.Timings.Add(new BusinessEntityTiming()
                {
                    Day = workingDay.Day,
                    IsActive = workingDay.IsActive
                });
            }

            return businessEntity;
        }

        private User CreateUserFromModel(CreateBusinessEntityRequest model)
        {
            var user = new User()
            {
                Password = model.AdminUserName,
                UserName = model.AdminUserName,
                //TenantId = 1,
                BranchId = model.BranchId,
                CreationType = CreationType.USER,
                OtpValidated = false,
            };

            switch (model.Type)
            {
                case Shared.Enums.BusinessEntityType.Relaypoint:
                    user.Type = UserType.RELAY_POINT;
                    break;
                case Shared.Enums.BusinessEntityType.Distributor:
                    user.Type = UserType.DISTRIBUTOR;
                    break;
                case Shared.Enums.BusinessEntityType.Dealer:
                    user.Type = UserType.DEALER;
                    break;
                case Shared.Enums.BusinessEntityType.Alds:
                    user.Type = UserType.ALDS_ADMIN;
                    break;
                case Shared.Enums.BusinessEntityType.CarWash:
                    user.Type = UserType.CARWASH_ADMIN;
                    break;
                case Shared.Enums.BusinessEntityType.Lubs:
                    user.Type = UserType.LUBS_ADMIN;
                    break;
                default:
                    break;
            }

            return user;
        }

        private UserProfile CreateProfileFromModel(CreateBusinessEntityRequest model)
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
                yield return "Relaypoint not found";
            }
            if (_model.Name == null)
            {
                yield return "Name is empty";
            }
            if (_model.MobileNumber == null)
            {
                yield return "Mobile Number is empty";
            }

            if (_model.BranchId <= 0)
            {
                yield return "Branch is invalid";
            }
        }
    }
}

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

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateRelaypointCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public BusinessEntity BusinessEntity { get; }

        public bool ValidateOnly { get; }

        private readonly CreateRelaypointRequest _model;

        public UpdateRelaypointCommand(CreateRelaypointRequest model, bool validateOnly = false)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model);
            BusinessEntity = CreateBusinessEntityFromModel(model);

            ValidateOnly = validateOnly;
        }

        private BusinessEntity CreateBusinessEntityFromModel(CreateRelaypointRequest model)
        {
            var businessEntity = new BusinessEntity()
            {
                Id = (int)model.Id,
                Type = model.Type,
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                Code = model.Code,
                Name = model.Name,
                MobileNumber = model.MobileNumber,
                Email = model.Email,
                WorkingStartDay = model.WorkingStartDay,
                WorkingEndDay = model.WorkingEndDay,
                WorkingStartTime = model.WorkingStartTime,
                WorkingEndTime = model.WorkingEndTime,
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

        private User CreateUserFromModel(CreateRelaypointRequest model)
        {
            var user = new User()
            {
                Password = model.AdminUserName,
                UserName = model.AdminUserName,
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                CreationType = CreationType.USER,
                Type = UserType.RELAY_POINT,
                OtpValidated = false,
            };
            return user;
        }

        private UserProfile CreateProfileFromModel(CreateRelaypointRequest model)
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
                AgreedTerms = true
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

            if (_model.TenantId <= 0)
            {
                yield return "Tenant is invalid";
            }
            if (_model.BranchId <= 0)
            {
                yield return "Branch is invalid";
            }
        }
    }
}

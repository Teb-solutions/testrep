using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Distributor;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateDealerCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public BusinessEntity BusinessEntity { get; }

        public bool ValidateOnly { get; }

        private readonly CreateDealerRequest _model;

        public CreateDealerCommand(CreateDealerRequest model, BusinessEntityModel businessEntityModel, bool validateOnly = false)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model, businessEntityModel.TenantId, businessEntityModel.BranchId);
            BusinessEntity = CreateBusinessEntityFromModel(model, businessEntityModel);

            ValidateOnly = validateOnly;

            Profile.User = User;
            User.Profile = Profile;
            User.BusinessEntity = BusinessEntity;
        }

        private BusinessEntity CreateBusinessEntityFromModel(CreateDealerRequest model, BusinessEntityModel businessEntityModel)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            var businessEntity = new BusinessEntity()
            {
                Type = BusinessEntityType.Dealer,
                TenantId = businessEntityModel.TenantId,
                BranchId = businessEntityModel.BranchId,
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
                GeoLocation = geometryFactory.CreatePoint(new Coordinate(model.Lng, model.Lat)),
                Landmark = model.Landmark,
                Details = model.Details,
                PinCode = model.PinCode,
                State = model.State,
                ParentBusinessEntityId = businessEntityModel.Id,
                Rating = 5,
                IsActive = true,
                Timings = new List<BusinessEntityTiming>()
            };

            return businessEntity;
        }

        private User CreateUserFromModel(CreateDealerRequest model, int tenantId, int branchId)
        {
            var user = new User()
            {
                Password = model.AdminPassword,
                UserName = model.AdminUserName,
                TenantId = tenantId,
                BranchId = branchId,
                CreationType = CreationType.USER,
                Type = UserType.DEALER,
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
                FirstName = model.AdminFirstName,
                LastName = model.AdminLastName,
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
            if (_model.AdminUserName == null)
            {
                yield return "Username is empty";
            }
            
            if (_model.AdminPassword == null)
            {
                yield return "Password is empty";
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

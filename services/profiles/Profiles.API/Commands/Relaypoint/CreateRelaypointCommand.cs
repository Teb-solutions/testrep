﻿using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;
using Microsoft.SqlServer.Types;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateRelaypointCommand : CommandBase
    {
        public UserProfile Profile { get; }
        public User User { get; }
        public BusinessEntity BusinessEntity { get; }

        public bool ValidateOnly { get; }

        private readonly CreateRelaypointRequest _model;

        public CreateRelaypointCommand(CreateRelaypointRequest model, bool validateOnly = false)
        {
            _model = model;
            Profile = CreateProfileFromModel(model);
            User = CreateUserFromModel(model);
            BusinessEntity = CreateBusinessEntityFromModel(model);

            ValidateOnly = validateOnly;

            Profile.User = User;
            User.Profile = Profile;
            User.BusinessEntity = BusinessEntity;
        }

        private BusinessEntity CreateBusinessEntityFromModel(CreateRelaypointRequest model)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            var businessEntity = new BusinessEntity()
            {
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
                GeoLocation = geometryFactory.CreatePoint(new Coordinate(model.Lng, model.Lat)),
                Landmark = model.Landmark,
                Details = model.Details,
                PinCode = model.PinCode,
                State = model.State,
                ParentBusinessEntityId = model.ParentBusinessEntityId,
                Rating = 5,
                IsActive = true,
                Timings = new List<BusinessEntityTiming>()
            };

            foreach(var workingDay in model.WorkingDaysList)
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
                Password = model.AdminPassword,
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
                FirstName = model.AdminFirstName,
                LastName = model.AdminLastName,
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
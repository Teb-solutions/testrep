﻿using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Logging;
using Profiles.API.Models;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateRelaypointCommandHandler : ICommandHandler<CreateRelaypointCommand>
    {
        private readonly ProfilesDbContext _db;
        private VoucherMgr _voucherMgr;
        private ILogger _logger;

        public CreateRelaypointCommandHandler(ProfilesDbContext db, VoucherMgr voucherMgr, ILogger<CreateProfileAndUserCommandHandler> logger)
        {
            _db = db;
            _voucherMgr = voucherMgr;
            _logger = logger;
        }

        public CommandHandlerResult Handle(CreateRelaypointCommand command)
        {
            var duplicateBusinessEntity = _db.BusinessEntities.Any(x => x.Name == command.BusinessEntity.Name && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntity)
            {
                return CommandHandlerResult.Error($"Relaypoint with name ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityMobile = _db.BusinessEntities.Any(x => x.MobileNumber == command.BusinessEntity.MobileNumber && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityMobile)
            {
                return CommandHandlerResult.Error($"Relaypoint with mobile ({command.BusinessEntity.Name}) already exists.");
            }

            var existing = _db.Users.Any(x => x.UserName == command.User.UserName && x.Type == command.User.Type);
            if (existing)
            {
                return CommandHandlerResult.Error($"User with username ({command.User.UserName}) already exists.");
            }

            var tenant = _db.Tenants.Where(p => p.Id == command.BusinessEntity.TenantId).FirstOrDefault();
            if (tenant == null)
            {
                return CommandHandlerResult.Error($"Tenant is invalid.");
            }

            var branch = _db.Branches.Where(p => p.Id == command.BusinessEntity.BranchId).FirstOrDefault();
            if (branch == null)
            {
                return CommandHandlerResult.Error($"Branch is invalid.");
            }

            if (command.ValidateOnly)
            {
                return CommandHandlerResult.Ok;
            }

            if (!string.IsNullOrEmpty(command.User.Password))
            {
                command.User.Password = Helper.HashPassword(command.User.Password);
            }

            // generate my referral code for user
            string myReferralCode = _voucherMgr.GenerateMyReferralCode();
            if (string.IsNullOrEmpty(myReferralCode))
            {
                myReferralCode = command.User.Profile.Mobile;
            }
            command.User.Profile.MyReferralCode = myReferralCode;

            _db.BusinessEntities.Add(command.BusinessEntity);

            var role = _db.Roles.Where(p => p.Name == RoleNames.RELAYPOINT_ADMIN).FirstOrDefault();
            if (role == null)
            {
                return CommandHandlerResult.Error($"Role is not set.");
            }
            command.User.Roles.Add(new UserRole(role.Id));

            _db.Users.Add(command.User);

            _logger.LogInformation("New Relaypoint Created | Name: " + command.BusinessEntity.Name);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateRelaypointResponse
                {
                    Id = command.BusinessEntity.Id,
                });
        }
    }
}

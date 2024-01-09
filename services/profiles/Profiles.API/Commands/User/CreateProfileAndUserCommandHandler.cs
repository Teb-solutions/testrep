using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Profiles.API.Models;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateProfileAndUserCommandHandler : ICommandHandler<CreateProfileAndUserCommand>
    {
        private readonly ProfilesDbContext _db;
        private CreateProfileAndUserCommand _command;
        private VoucherMgr _voucherMgr;
        private GeoFenceMgr _geoFenceMgr;
        private ILogger _logger;

        public CreateProfileAndUserCommandHandler(ProfilesDbContext db, VoucherMgr voucherMgr, GeoFenceMgr geoFenceMgr, ILogger<CreateProfileAndUserCommandHandler> logger)
        {
            _db = db;
            _voucherMgr = voucherMgr;
            _geoFenceMgr = geoFenceMgr;
            _logger = logger;
        }

        public CommandHandlerResult Handle(CreateProfileAndUserCommand command)
        {
            _command = command;
            var existing = _db.Users.Any(x => x.UserName == command.User.UserName && x.Type == _command.User.Type);
            if (existing)
            {
                return CommandHandlerResult.Error($"Username ({command.User.UserName}) already exists.");
            }

            if (!string.IsNullOrEmpty(_command.Profile.Code))
            {
                var existingCode = _db.Profiles.Any(x => x.Code == _command.Profile.Code);
                if (existingCode)
                {
                    return CommandHandlerResult.Error($"Code ({_command.Profile.Code}) already exists.");
                }
            }

            
            if (!string.IsNullOrEmpty(_command.Profile.ReferralCode))
            {
                int? referredUserId = null;
                var refCode = _command.Profile.ReferralCode.ToLower();
                var referredUserProfile = _db.Profiles.Where(x => x.MyReferralCode == refCode).FirstOrDefault();
                if (referredUserProfile == null)
                {
                    //referral code can be mobile number also
                    var referredUser = _db.Users.Where(x => x.UserName == _command.Profile.ReferralCode).FirstOrDefault();
                    if (referredUser == null)
                    {
                        return CommandHandlerResult.Error($"Referral Code ({_command.Profile.ReferralCode}) does not exist.");
                    }
                    referredUserId = referredUser.Id;
                }
                else
                {
                    referredUserId = referredUserProfile.UserId;
                }
                if (referredUserId != null)
                {
                    command.User.Profile.ReferredByUserId = referredUserId;
                }
            }

            var tenant = _db.Tenants.Where(p => p.Id == _command.User.TenantId).FirstOrDefault();
            if (tenant == null)
            {
                return CommandHandlerResult.Error($"Tenant is invalid.");
            }

            if (_command.User.BranchId != null)
            {
                var branch = _db.Branches.Where(p => p.Id == _command.User.BranchId).FirstOrDefault();
                if (branch == null)
                {
                    return CommandHandlerResult.Error($"Branch is invalid.");
                }
            }

            if (_command.User.BusinessEntityId > 0)
            {
                var businessEntity = _db.BusinessEntities.Where(p => p.Id == _command.User.BusinessEntityId).FirstOrDefault();
                if (businessEntity == null)
                {
                    return CommandHandlerResult.Error($"Business Entity is invalid.");
                }
            }

            if (_command.ValidateOnly)
            {
                return CommandHandlerResult.Ok;
            }

            if (!string.IsNullOrEmpty(command.User.Password))
            {
                command.User.Password = Helper.HashPassword(command.User.Password);
            }
            
            // generate my referral code
            string myReferralCode = _voucherMgr.GenerateMyReferralCode();
            if (string.IsNullOrEmpty(myReferralCode))
            {
                myReferralCode = command.User.Profile.Mobile;
            }
            command.User.Profile.MyReferralCode = myReferralCode;
            
            List<UserType> staffUserTypes = new List<UserType>() 
            {
                UserType.DRIVER, UserType.SECURITY, UserType.MARSHAL, UserType.PICKUP_DRIVER
            };

            if (command.User.Type == Shared.Enums.UserType.CUSTOMER)
            {
                var role = _db.Roles.Where(p => p.Name == RoleNames.CUSTOMER).FirstOrDefault();
                if (role == null)
                {
                    return CommandHandlerResult.Error($"Role is not set.");
                }
                command.User.Roles.Add(new UserRole(role.Id));

                if (_command.Profile.RegisteredFromLat != null && _command.Profile.RegisteredFromLng != null)
                {
                    Branch branch = _geoFenceMgr.GetBranchByLatLng((double)_command.Profile.RegisteredFromLat, (double)_command.Profile.RegisteredFromLng).Result;
                    if (branch != null)
                    {
                        _command.User.BranchId = branch.Id;
                    }
                }
            }
            else if (staffUserTypes.Contains(command.User.Type))
            {
                var roleName = new RoleNames().GetRoleOfUserType(command.User.Type);
                var role = _db.Roles.Where(p => p.Name == roleName).FirstOrDefault();
                if (role == null)
                {
                    return CommandHandlerResult.Error($"Role is not set.");
                }
                command.User.Roles.Add(new UserRole(role.Id));
            }
            else
            {
                return CommandHandlerResult.Error($"User type is not set.");
            }

            _db.Users.Add(command.User);
            _logger.LogInformation("CreateProfileAndUserCommandHandler New User Created | userType: " + command.User.Type + "username: " + command.User.UserName);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateProfileResponse
                {
                    UserId = _command.User.Id,
                    ProfileId = _command.Profile.Id
                });
        }
    }
}

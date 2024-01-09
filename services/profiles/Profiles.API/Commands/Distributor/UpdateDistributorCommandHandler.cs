using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Profiles.API.ViewModels.Relaypoint;
using System.Linq;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Distributor;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateDistributorCommandHandler : ICommandHandler<UpdateDistributorCommand>
    {
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateDistributorCommandHandler(ProfilesDbContext db, VoucherMgr voucherMgr, ILogger<CreateProfileAndUserCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UpdateDistributorCommand command)
        {

            var businessEntity = _db.BusinessEntities
                .Include(p => p.Timings)
                .Where(p => p.Id == command.BusinessEntity.Id)
                .FirstOrDefault();
            if (businessEntity == null)
            {
                return CommandHandlerResult.Error($"Distributor does not exists.");
            }

            var duplicateBusinessEntity = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.Name == command.BusinessEntity.Name && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntity)
            {
                return CommandHandlerResult.Error($"Distributor with name ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityMobile = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.MobileNumber == command.BusinessEntity.MobileNumber && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityMobile)
            {
                return CommandHandlerResult.Error($"Distributor with mobile ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityCode = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.Code == command.BusinessEntity.Code && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityCode)
            {
                return CommandHandlerResult.Error($"Distributor with code ({command.BusinessEntity.Code}) already exists.");
            }

            var duplicateBusinessEntityCodeProfile = _db.Profiles.Include(p => p.User).Any(x => x.User.Type != UserType.DISTRIBUTOR && x.User.BusinessEntityId != command.BusinessEntity.Id && x.MyReferralCode == command.BusinessEntity.Code);
            if (duplicateBusinessEntityCodeProfile)
            {
                return CommandHandlerResult.Error($"User with code ({command.BusinessEntity.Code}) already exists.");
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

            var relaypointUser = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.DISTRIBUTOR && p.BusinessEntityId == businessEntity.Id).FirstOrDefault();
            if (relaypointUser == null)
            {
                return CommandHandlerResult.Error($"User is invalid.");
            }

            if (command.ValidateOnly)
            {
                return CommandHandlerResult.Ok;
            }

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            businessEntity.TenantId = command.BusinessEntity.TenantId;
            businessEntity.BranchId = command.BusinessEntity.BranchId;
            businessEntity.Code = command.BusinessEntity.Code;
            businessEntity.Name = command.BusinessEntity.Name;
            businessEntity.MobileNumber = command.BusinessEntity.MobileNumber;
            businessEntity.Email = command.BusinessEntity.Email;
            businessEntity.GSTN = command.BusinessEntity.GSTN;
            businessEntity.PAN = command.BusinessEntity.PAN;
            businessEntity.PaymentNumber = command.BusinessEntity.PaymentNumber;
            businessEntity.Location = command.BusinessEntity.Location;
            businessEntity.Lat = command.BusinessEntity.Lat;
            businessEntity.Lng = command.BusinessEntity.Lng;
            businessEntity.GeoLocation = geometryFactory.CreatePoint(new Coordinate(command.BusinessEntity.Lng, command.BusinessEntity.Lat));
            businessEntity.Landmark = command.BusinessEntity.Landmark;
            businessEntity.Details = command.BusinessEntity.Details;
            businessEntity.PinCode = command.BusinessEntity.PinCode;
            businessEntity.State = command.BusinessEntity.State;
            businessEntity.ParentBusinessEntityId = command.BusinessEntity.ParentBusinessEntityId;
            //businessEntity.Rating = command.BusinessEntity.Rating;
            businessEntity.IsActive = command.BusinessEntity.IsActive;

            if (businessEntity.Timings != null)
            {
                foreach (var workingDay in businessEntity.Timings)
                {
                    _db.BusinessEntityTimings.Remove(workingDay);
                }
            }

            businessEntity.Timings = command.BusinessEntity.Timings;

            relaypointUser.UserName = command.User.UserName;
            relaypointUser.BranchId = command.User.BranchId;

            relaypointUser.Profile.FirstName = command.Profile.FirstName;
            relaypointUser.Profile.LastName = command.Profile.LastName;
            relaypointUser.Profile.Email = command.Profile.Email;
            relaypointUser.Profile.Mobile = command.Profile.Mobile;
            relaypointUser.Profile.Code = command.Profile.Code;
            relaypointUser.Profile.MyReferralCode = command.Profile.MyReferralCode;

            _logger.LogInformation("Distributor Updated | Name: " + command.BusinessEntity.Name);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateDistributorResponse
                {
                    Id = command.BusinessEntity.Id,
                });
        }
    }
}

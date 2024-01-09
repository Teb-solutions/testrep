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

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateRelaypointCommandHandler : ICommandHandler<UpdateRelaypointCommand>
    {
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateRelaypointCommandHandler(ProfilesDbContext db, VoucherMgr voucherMgr, ILogger<CreateProfileAndUserCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UpdateRelaypointCommand command)
        {

            var businessEntity = _db.BusinessEntities
                .Include(p => p.Timings)
                .Where(p => p.Id == command.BusinessEntity.Id)
                .FirstOrDefault();
            if (businessEntity == null)
            {
                return CommandHandlerResult.Error($"Relaypoint does not exists.");
            }

            var duplicateBusinessEntity = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.Name == command.BusinessEntity.Name && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntity)
            {
                return CommandHandlerResult.Error($"Relaypoint with name ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityMobile = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.MobileNumber == command.BusinessEntity.MobileNumber && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityMobile)
            {
                return CommandHandlerResult.Error($"Relaypoint with mobile ({command.BusinessEntity.Name}) already exists.");
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

            var relaypointUser = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.RELAY_POINT && p.BusinessEntityId == businessEntity.Id).FirstOrDefault();
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
            businessEntity.WorkingStartDay = command.BusinessEntity.WorkingStartDay;
            businessEntity.WorkingEndDay = command.BusinessEntity.WorkingEndDay;
            businessEntity.WorkingStartTime = command.BusinessEntity.WorkingStartTime;
            businessEntity.WorkingEndTime = command.BusinessEntity.WorkingEndTime;
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

            _logger.LogInformation("Relaypoint Updated | Name: " + command.BusinessEntity.Name);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateRelaypointResponse
                {
                    Id = command.BusinessEntity.Id,
                });
        }
    }
}

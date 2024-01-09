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
    public class UpdateDealerCommandHandler : ICommandHandler<UpdateDealerCommand>
    {
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateDealerCommandHandler(ProfilesDbContext db, VoucherMgr voucherMgr, ILogger<CreateProfileAndUserCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UpdateDealerCommand command)
        {

            var businessEntity = _db.BusinessEntities
                .Include(p => p.Timings)
                .Where(p => p.Id == command.BusinessEntity.Id)
                .FirstOrDefault();
            if (businessEntity == null)
            {
                return CommandHandlerResult.Error($"Dealer does not exists.");
            }

            if (businessEntity.ParentBusinessEntityId != command.BusinessEntity.ParentBusinessEntityId)
            {
                return CommandHandlerResult.Error($"Some error has occurred.");
            }

            var duplicateBusinessEntity = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.Name == command.BusinessEntity.Name && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntity)
            {
                return CommandHandlerResult.Error($"Dealer with name ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityMobile = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.MobileNumber == command.BusinessEntity.MobileNumber && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityMobile)
            {
                return CommandHandlerResult.Error($"Dealer with mobile ({command.BusinessEntity.Name}) already exists.");
            }

            var duplicateBusinessEntityCode = _db.BusinessEntities.Any(x => x.Id != command.BusinessEntity.Id && x.Code == command.BusinessEntity.Code && x.Type == command.BusinessEntity.Type);
            if (duplicateBusinessEntityCode)
            {
                return CommandHandlerResult.Error($"Dealer with code ({command.BusinessEntity.Code}) already exists.");
            }

            var duplicateBusinessEntityCodeProfile = _db.Profiles.Include(p => p.User).Any(x => x.User.Type != UserType.DISTRIBUTOR && x.User.BusinessEntityId != command.BusinessEntity.Id && x.MyReferralCode == command.BusinessEntity.Code);
            if (duplicateBusinessEntityCodeProfile)
            {
                return CommandHandlerResult.Error($"User with code ({command.BusinessEntity.Code}) already exists.");
            }

            var dealerUser = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.DISTRIBUTOR && p.BusinessEntityId == businessEntity.Id).FirstOrDefault();
            if (dealerUser == null)
            {
                return CommandHandlerResult.Error($"User is invalid.");
            }

            if (command.ValidateOnly)
            {
                return CommandHandlerResult.Ok;
            }

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            //businessEntity.TenantId = command.BusinessEntity.TenantId;
            //businessEntity.BranchId = command.BusinessEntity.BranchId;
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

            dealerUser.UserName = command.User.UserName;
            //dealerUser.BranchId = command.User.BranchId;

            dealerUser.Profile.FirstName = command.Profile.FirstName;
            dealerUser.Profile.LastName = command.Profile.LastName;
            dealerUser.Profile.Email = command.Profile.Email;
            dealerUser.Profile.Mobile = command.Profile.Mobile;
            dealerUser.Profile.Code = command.Profile.Code;
            dealerUser.Profile.MyReferralCode = command.Profile.MyReferralCode;

            _logger.LogInformation("Dealer {name} Updated {@dealer}", command.BusinessEntity.Name, command.BusinessEntity);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateDistributorResponse
                {
                    Id = command.BusinessEntity.Id,
                });
        }
    }
}

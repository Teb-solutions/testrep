using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System.Linq;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateVehicleCommandHandler : ICommandHandler<UpdateVehicleCommand>
    {
        private readonly ProfilesDbContext _db;
        private UpdateVehicleCommand _command;
        public UpdateVehicleCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }

        public CommandHandlerResult Handle(UpdateVehicleCommand command)
        {
            _command = command;
            var patching = _command.IsPatch;
            var vehicle = _command.Vehicle;
            var existingVeh = _db.Vehicles.Where(p => p.Id == vehicle.Id).FirstOrDefault();
            if (existingVeh == null)
            {
                return CommandHandlerResult.Error($"Invalid vehicle");
            }

            var duplicateVeh = _db.Vehicles.Any(x => x.RegNo == vehicle.RegNo && x.Id != vehicle.Id);
            if (duplicateVeh)
            {
                return CommandHandlerResult.Error($"Reg No already exists ({_command.Vehicle.RegNo})");
            }

            if (vehicle.DriverId > 0)
            {
                var driver = _db.Users.Where(p => p.Id == vehicle.DriverId && p.Type == UserType.DRIVER).SingleOrDefault();
                if (driver == null)
                {
                    return CommandHandlerResult.Error($"Driver does not exist");
                }
                existingVeh.DriverId = vehicle.DriverId;
            }
            else
            {
                existingVeh.DriverId = null;
            }

                var distributor = _db.BusinessEntities.Where(p => p.Id == vehicle.BusinessEntityId).SingleOrDefault();
                if (distributor == null)
                {
                    return CommandHandlerResult.Error($"Distributor does not exist");
                }
                existingVeh.BusinessEntityId = vehicle.BusinessEntityId;

            if (!patching)
            {
                existingVeh.RegNo = vehicle.RegNo;
                existingVeh.OriginLat = vehicle.OriginLat;
                existingVeh.OriginLng = vehicle.originLng;
                existingVeh.DestinationLat = vehicle.DestinationLat;
                existingVeh.DestinationLng = vehicle.DestinationLng;
                existingVeh.IsActive = vehicle.IsActive;
                return CommandHandlerResult.Ok;
            }
            else return PatchVehicle(existingVeh, vehicle);
        }

        private CommandHandlerResult PatchVehicle(Vehicle existing, AddVehicleRequest vehicle)
        {
            if (!string.IsNullOrEmpty(vehicle.RegNo))
            {
                existing.RegNo = vehicle.RegNo;
            }

            return CommandHandlerResult.Ok;
        }
    }
}

using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using System.Linq;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand>
    {
        private readonly ProfilesDbContext _db;
        private CreateVehicleCommand _command;
        public CreateVehicleCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }

        public CommandHandlerResult Handle(CreateVehicleCommand command)
        {
            _command = command;
            var duplicateVeh = _db.Vehicles.Any(x => x.RegNo == _command.Vehicle.RegNo);
            if (duplicateVeh)
            {
                return CommandHandlerResult.Error($"Reg No already exists ({command.Vehicle.RegNo})");
            }

            if (_command.Vehicle.DriverId > 0)
            {
                var driver = _db.Users.Where(p => p.Id == _command.Vehicle.DriverId && p.Type == UserType.DRIVER).SingleOrDefault();
                if (driver == null)
                {
                    return CommandHandlerResult.Error($"Driver does not exist");
                }
            }
            else
            {
                _command.Vehicle.DriverId = null;
            }


                var distributor = _db.BusinessEntities.Where(p => p.Id == _command.Vehicle.BusinessEntityId && p.Type == BusinessEntityType.Distributor).SingleOrDefault();
                if (distributor == null)
                {
                    return CommandHandlerResult.Error($"Distributor does not exist");
                }

            var tenant = _db.Tenants.Where(p => p.Id == _command.Vehicle.TenantId).FirstOrDefault();
            if (tenant == null)
            {
                return CommandHandlerResult.Error($"Tenant is invalid.");
            }

                var branch = _db.Branches.Where(p => p.Id == _command.Vehicle.BranchId).FirstOrDefault();
                if (branch == null)
                {
                    return CommandHandlerResult.Error($"Branch is invalid.");
                }

            command.Vehicle.MaxCylinders = 30; //TODO remove hardcode
            _db.Vehicles.Add(command.Vehicle);
            return CommandHandlerResult.OkDelayed(this,
                x => new
                {
                    VehicleId = _command.Vehicle.Id,
                });
        }
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateVehicleCommand : CommandBase
    {
        public Vehicle Vehicle;
        public CreateVehicleCommand(AddVehicleRequest model)
        {
            Vehicle = CreateVehicleFromModel(model);
        }

        private Vehicle CreateVehicleFromModel(AddVehicleRequest model)
        {
            var vehicle = new Vehicle()
            {
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                DriverId = model.DriverId,
                BusinessEntityId = model.BusinessEntityId,
                RegNo = model.RegNo,
                OriginLat = model.OriginLat,
                OriginLng = model.originLng,
                DestinationLat = model.DestinationLat,
                DestinationLng = model.DestinationLng,
                MaxCylinders = model.MaxCylinders,
                IsActive = model.IsActive
            };

            return vehicle;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Vehicle == null)
            {
                yield return "Invalid or no payload received";
            }
            else
            {
                if (String.IsNullOrEmpty(Vehicle.RegNo))
                {
                    yield return "Reg No is missing";
                }
                //if (Vehicle.DistributorId <= 0 || Vehicle.DistributorId == null)
                //{
                //    yield return "Distributor is invalid";
                //}
                if (Vehicle.OriginLat < -90 || Vehicle.OriginLat > 90)
                {
                    yield return "Invalid Origin Lat";
                }
                if (Vehicle.OriginLng < -180 || Vehicle.OriginLng > 180)
                {
                    yield return "Invalid Origin Lng";
                }
                if (Vehicle.DestinationLat < -90 || Vehicle.DestinationLat > 90)
                {
                    yield return "Invalid Destination Lat";
                }
                if (Vehicle.DestinationLng < -180 || Vehicle.DestinationLng > 180)
                {
                    yield return "Invalid Destination Lng";
                }
            }
        }
    }
}

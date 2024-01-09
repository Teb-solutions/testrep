using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Core.Commands;
using System.Collections.Generic;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateVehicleCommand : CommandBase
    {
        public bool IsPatch { get; }
        public AddVehicleRequest Vehicle { get; }
        public UpdateVehicleCommand(AddVehicleRequest vehicle, bool isPatch)
        {
            Vehicle = vehicle;
            IsPatch = isPatch;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Vehicle.Id <= 0)
            {
                yield return "Vehicle id not found";
            }

            if (!IsPatch)
            {
                foreach (var msg in FullValidation())
                {
                    yield return msg;
                }
            }
        }

        private IEnumerable<string> FullValidation()
        {
            if (string.IsNullOrEmpty(Vehicle.RegNo))
            {
                yield return "Reg No is missing";
            }
            if (Vehicle.BusinessEntityId <= 0 || Vehicle.BusinessEntityId == null)
            {
                yield return "Relaypoint is invalid";
            }
            if (Vehicle.OriginLat < -90 || Vehicle.OriginLat > 90)
            {
                yield return "Invalid Origin Lat";
            }
            if (Vehicle.originLng < -180 || Vehicle.originLng > 180)
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

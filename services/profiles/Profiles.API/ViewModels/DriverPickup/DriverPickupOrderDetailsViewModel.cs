using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;

namespace Profiles.API.Models.DriverPickupOrder
{
    public class DriverPickupOrderDetailsViewModel
    {
        public BusinessEntityModel Relaypoint { get; set; }
        public DriverProfileModel Profile { get; set; }
    }
}

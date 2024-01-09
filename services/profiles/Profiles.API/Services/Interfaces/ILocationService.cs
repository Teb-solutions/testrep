using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface ILocationService
    {
        Task<List<VehicleLocationsViewModel>> GetVehicleLocations(int tenantId);
        Task<VehicleLocationsViewModel> GetVehicleLocation(int vehicleId);
    }
}

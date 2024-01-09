using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IVehicleQueries
    {
        Task<IEnumerable<Vehicle>> GetAllAsync(int? tenantId, int? branchId, int? distributorId);
        Task<List<VehicleModel>> GetAllList(int? tenantId, int? branchId, int? distributorId, bool includeDriverActivity = false);
        Task<List<BuildingBlocks.EventBus.Events.VehicleModel>> GetAllListForPlanning(int? tenantId, int? branchId, int? distributorId, bool includeDriverActivity = false);

        Task<VehicleModel> GetDetails(int id, bool includeDriverActivity = false);
        int GetDriversVehicleId(int driverId);
    }
}

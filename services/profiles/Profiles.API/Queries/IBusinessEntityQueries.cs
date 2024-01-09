using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Profiles.API.Models;
using Profiles.API.ViewModels.BusinessEntity;
using Profiles.API.ViewModels.CartAggregate;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.ViewModels.DriverPickup;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IBusinessEntityQueries
    {
        Task<List<BusinessEntityModel>> GetAllList(BusinessEntityType? type = null, bool? isActive = null, int? tenantId = null, int? branchId = null, int? parentId = null, double? lat = null, double? lng = null, double? radius = 999999999);
        Task<List<CartRelaypoint>> GetRelaypointListForCustomerCart(int? branchId, double? lat, double? lng, double? radius = null);

        Task<BusinessEntityModel> GetDetailsById(int id);
        Task<List<SelectListItem>> GetRelaypointSelectList(int? tenantId, int? branchId);
        Task<User> GetRelaypointAdminUser(int relaypointId);

        Task<DriverPickupRelaypointDetails> GetRelaypointDetailsForDriverPickup(int relaypointId);

        Task<List<DriverPickupRelaypointDetails>> GetRelaypointListForDriverPickup(int driverId, int orderId, double? originLat, double? originLng, double? radius = null);

        Task<RelaypointProfile> GetRelaypointProfile(int relaypointId);
        Task<UpdateRelaypointAddress> GetRelaypointAddress(int relaypointId);
        Task<List<WorkingDaysModel>> GetRelaypointWorkingDays(int relaypointId);
        Task<UpdateWorkingTimeRequest> GetRelaypointWorkingTime(int relaypointId);

        Task<List<SelectListItem>> GetSelectList(BusinessEntityType type, int? tenantId, int? branchId);
        Task<User> GetAdminUser(int id, UserType type);
        Task<DistributorProfile> GetProfile(int businessEntityId);
        Task<UpdateDistributorAddress> GetAddress(int businessEntityId);

        Task<DistributorDashboardModel> GetBusinessEntityDashboard(int id, int userId);

        Task<CreateBusinessEntityRequest> GetDetailsByIdForUpdate(int id);

        Task<List<Device>> GetDevices(int entityId);
        Task<Device> GetDeviceById(int id);
        Task<List<Device>> GetDevicesByParentId(int id);
    }
}

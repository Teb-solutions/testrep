using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Core.Commands;
using Profiles.API.Models;
using Profiles.API.ViewModels;
using EasyGas.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Profiles.API.ViewModels.Distributor;
using Microsoft.AspNetCore.Mvc;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IProfileQueries
    {
        Task<IEnumerable<UserDetailsModel>> GetCustomersBySearch(string term, int from, int size, int? tenantId, int? branchId, int? businessEntityId = null);
        Task<IEnumerable<UserAndProfileModel>> GetCustomersList(DateTime from, DateTime to, int? tenantId, int? branchId);
        Task<UserAndProfileModel> GetByUserId(int userid);
        Task<UserAndProfileModel> GetByCode(string code);
        Task<CustomerProfileModel> GetCustomerProfileByUserId(int userid);
        Task<DriverProfileModel> GetDriverProfileByUserId(int userid);
        Task<UserDetailsModel> GetDetailsByUserId(int userid);
        Task<UserProfile> GetByMobile(string value);
        Task<UserProfile> GetByEmail(string value);
        Task<UserAndProfileModel> GetUserAndProfileByMobile(string value, UserType userType, int tenantId);
        int GetCountByCreationDate(DateTime? fromDate, DateTime? toDate, UserType? type, int? tenantId, int? branchId);
        Task<List<DriverModel>> GetDriverList(int? tenantId, int? branchId, int? businessEntityId);
        Task<List<SelectListItem>> GetDriverSelectList(int? tenantId, int? branchId, int? businessEntityId);

        Task<List<StaffModel>> GetStaffList(int? tenantId, int? branchId, int? businessEntityId);
        Task<StaffModel> GetStaffDetail(int staffId);

        Task<List<DistributorModel>> GetDistributorList(int? tenantId, int? branchId);

        Task<IEnumerable<AddressModel>> GetUserAddressList(int userId);
        Task<AddressModel> GetUserAddress(int useAddressId, int userId);

        //Task<CloudConfSync> GetCloudConfig(int tenantId, int? branchId, int? driverId);
        Task<CommandResult> GetAppConfig(int userId, UserType type);
        Task<CustomerAppConfig> GetCustomerAppConfig(int? branchId);
        Task<CommandResult> GetDriverAppConfig(int userId);
        Task<List<AppImage>> GetAppImages(int? tenantId, int? branchId);
        Task<int> GetInstalledNotRegisteredCustomerCount(int tenantId, int? branchId);

        Task<DeliverySlotModel> GetDeliverySlot(int Id);
        Task<IEnumerable<DeliverySlotModel>> GetDeliverySlotList(int branchId);

        Task<List<CustomerComplaintModel>> GetCustomerTickets(int? userId = null, int? tenantId = null, int? branchId = null);
        Task<PvtWebDashboardVM> GetCrmTicketInfoForAdminDashboard(int? tenantId = null);

        Task<IEnumerable<BackendUserProfileModel>> GetBackendUsers(int tenantId, int? branchId);

        Task<List<DistributorCustomerModel>> GetCustomersListOfBusinessEntity(int businessEntityId, int distributerUserId);
    }
}

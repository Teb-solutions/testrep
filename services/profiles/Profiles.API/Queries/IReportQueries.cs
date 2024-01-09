using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IReportQueries
    {
        Task<DriverActivitySummaryVM> GetDriverActivityReport(DateTime fromDate, DateTime toDate, int? tenantId, int? branchId, int? driverId);
        //Task<IEnumerable<DriverOrderSummaryVM>> GetDriverSummaryDatewiseList(DateTime fromDate, DateTime toDate, int? tenantId, int? branchId, int? driverId);

        //Task<IEnumerable<DriverOrderSummaryVM>> GetDriverSummaryList(DateTime fromDate, DateTime toDate, int? tenantId, int? branchId, int? driverId, int? vehicleId, bool includeActivityList);

        /*

        Task<DriverOrderSummaryVM> GetCurrentDriverSummary(int? driverId, int? vehicleId);
        Task<IEnumerable<DriverOrderSummaryVM>> GetDriverCurrentStatusList(int? tenantId, int? branchId, int? driverId, int? vehicleId);
        Task<IEnumerable<DriverActivity>> GetDriverActivityList(DateTime fromDate, DateTime toDate, int tenantId, int? branchId, int? driverId, int? vehicleId);
        //Task<List<OrderModel>> GetOrderSourceList(DateTime fromDate, DateTime toDate, int tenantId, int? branchId);
        Task<TransactionSummaryVM> GetDistributorTransactionList(int month, int year, int distributorId);
        */
    }
}

using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.BizLogic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.ViewModels.Report;

namespace EasyGas.Services.Profiles.Queries
{
    public class ReportQueries : IReportQueries
    {
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _db;
        private readonly VehicleMgr _vehMgr;
        private readonly ILogger _logger;
        //private IOrderQueries _orderQueries;

        public ReportQueries(IOptions<ApiSettings> apiSettings, ProfilesDbContext ctx, VehicleMgr vehMgr, ILoggerFactory loggerFactory)
        {
            _apiSettings = apiSettings;
            _db = ctx;
            _vehMgr = vehMgr;
            _logger = loggerFactory.CreateLogger<ReportQueries>();
            //_orderQueries = orderQueries;
        }

        public async Task<DriverActivitySummaryVM> GetDriverActivityReport(DateTime fromDate, DateTime toDate, int? tenantId, int? branchId, int? driverId)
        {
            var activitySummary = await _vehMgr.GetActivitySummary(tenantId, branchId, driverId, fromDate, toDate);
            return activitySummary;
        }


        /*
        public Task<IEnumerable<DriverOrderSummaryVM>> GetDriverSummaryList(DateTime fromDate, DateTime toDate, int? tenantId, int? branchId, int? driverId, int? vehicleId, bool includeActivityList = false)
        {
            List<DriverOrderSummaryVM> summaryList = new List<DriverOrderSummaryVM>();
            DateTime now = DateMgr.GetCurrentIndiaTime();
            try
            {
                List<Vehicle> vehList = _db.Vehicles.Include(p => p.Driver).Include(p => p.Driver.Profile).Include(p => p.Distributor).Where(p => p.IsActive == true).ToList();

                var orders = _db.Orders.Include(u => u.Driver).Include(u => u.Driver.Profile).Include(u => u.Vehicle).Include(u => u.Address).Where(p => p.VehicleId > 0).AsQueryable();
                if (fromDate != null)
                {
                    orders = orders.Where(p => p.DeliveryFrom.Date >= fromDate.Date);
                }
                if (toDate != null)
                {
                    orders = orders.Where(p => p.DeliveryFrom.Date <= toDate.Date);
                }
                if (driverId != null)
                {
                    orders = orders.Where(p => p.DriverId == driverId);
                    vehList = vehList.Where(p => p.DriverId == driverId).ToList();
                }
                if (vehicleId != null)
                {
                    orders = orders.Where(p => p.VehicleId == vehicleId);
                    vehList = vehList.Where(p => p.Id == vehicleId).ToList();
                }
                if (branchId != null)
                {
                    orders = orders.Where(p => p.BranchId == branchId);
                    vehList = vehList.Where(p => p.Distributor.BranchId == branchId).ToList();
                }
                if (tenantId != null)
                {
                    orders = orders.Where(p => p.TenantId == tenantId);
                    vehList = vehList.Where(p => p.Distributor.TenantId == tenantId).ToList();
                }
                var ordersList = orders.ToList();
                if (ordersList.Count > 0)
                {
                    orders = orders.Where(p => p.DriverId > 0);
                    Dictionary<int?, List<Order>> ordersGroupedBy = orders.GroupBy(p => p.DriverId).ToDictionary(p => p.Key, p => p.ToList());
                    foreach (KeyValuePair<int?, List<Order>> orderGrouped in ordersGroupedBy)
                    {
                        string driverName = orderGrouped.Value[0].Driver.Profile.GetFullName();
                        int totOrders = orderGrouped.Value.Count();
                        float totCash = orderGrouped.Value.Where(p => p.Status == OrderStatus.DELIVERED).Sum(p => p.TotalAmount);
                        int deliveredOrders = orderGrouped.Value.Where(p => p.Status == OrderStatus.DELIVERED).Count();
                        int deliveredDelayedOrders = orderGrouped.Value.Where(p => p.Status == OrderStatus.DELIVERED && p.DeliveredAt > p.DeliveryTo).Count();
                        int cancelledOrders = orderGrouped.Value.Where(p => p.Status == OrderStatus.CUSTOMER_CANCELLED || p.Status == OrderStatus.DRIVER_CANCELLED).Count();
                        int pendingDelayedOrders = orderGrouped.Value.Where(p => p.Status == OrderStatus.VEHICLE_ASSIGNED && p.DeliveryTo <= now).Count();
                        int pendingNotDelayedOrders = orderGrouped.Value.Where(p => p.Status == OrderStatus.VEHICLE_ASSIGNED && p.DeliveryTo > now).Count();
                        int ordersMissed = pendingDelayedOrders + pendingNotDelayedOrders;
                        var statusDriver = _db.DriverActivities.Where(p => p.UserId == orderGrouped.Key).OrderByDescending(x => x.Id).FirstOrDefault();
                        //var travelledMeters = _vehMgr.GetVehicleTravelledMetersByFormula(orderGrouped.Value[0].Vehicle.Id, fromDate, toDate);
                        long travelledMeters = 0;
                        if (orderGrouped.Value.Where(p => p.Status == OrderStatus.DELIVERED).Count() > 0)
                        {
                            travelledMeters = orderGrouped.Value.Where(p => p.Status == OrderStatus.DELIVERED).Sum(p => p.TripDistance);
                            if (travelledMeters == 0)
                            {
                                travelledMeters = _vehMgr.GetVehicleTravelledMetersByFormula(orderGrouped.Value, orderGrouped.Value[0].Vehicle.Id, orderGrouped.Key, fromDate, toDate);
                            }
                        }
                        //Random random = new Random();
                        //var randomNum = random.Next(4, 7);
                        DriverActivitySummaryVM activitySummary = _vehMgr.GetActivitySummary(orderGrouped.Key, fromDate, toDate);
                        if (!includeActivityList)
                        {
                            activitySummary.ActivityList = new List<DriverActivity>();
                        }
                        DriverOrderSummaryVM driverOrderSummary = new DriverOrderSummaryVM
                        {
                            Date = fromDate,
                            DriverId = (int)orderGrouped.Key,
                            DriverName = driverName,
                            VehicleName = orderGrouped.Value[0].Vehicle.RegNo,
                            OrdersDelivered = deliveredOrders,
                            OrdersDeliveredDelayed = deliveredDelayedOrders,
                            OrdersMissed = ordersMissed,
                            OrdersCancelled = cancelledOrders,
                            OrdersPendingDelayed = pendingDelayedOrders,
                            OrdersPendingNotDelayed = pendingNotDelayedOrders,
                            TravelledMeters = travelledMeters,
                            DriverActivitySummary = activitySummary,
                            ActiveTimeInMin = activitySummary.ActiveTimeInMin,
                            //ActiveTimeInMin = ((travelledMeters / 1000) * randomNum),
                            RewardPoints = 0,
                            StartPeriodUAT = 0,
                            EndPeriodUAT = 0,
                            CashCollected = totCash,
                            DaysWorked = 1,
                        };
                        if (statusDriver != null)
                        {
                            driverOrderSummary.Dls = statusDriver.dls;
                            driverOrderSummary.Das = statusDriver.das;
                            driverOrderSummary.DlsTime = statusDriver.CreatedAt;
                            driverOrderSummary.DasTime = statusDriver.CreatedAt;
                            driverOrderSummary.DlsTimeAgo = (int)now.Subtract(driverOrderSummary.DlsTime).TotalMinutes;
                            driverOrderSummary.DasTimeAgo = (int)now.Subtract(driverOrderSummary.DasTime).TotalMinutes;
                        }
                        summaryList.Add(driverOrderSummary);
                    }
                }

                if (vehList.Count > 0)
                {
                    foreach (var veh in vehList)
                    {
                        if (veh.DriverId > 0 && !summaryList.Any(p => p.DriverId == veh.DriverId))
                        {
                            var statusDriver = _db.DriverActivities.Where(p => p.UserId == veh.DriverId).OrderByDescending(x => x.Id).FirstOrDefault();

                            DriverOrderSummaryVM driverOrderSummary = new DriverOrderSummaryVM
                            {
                                DriverId = (int)veh.DriverId,
                                DriverName = veh.Driver.Profile.GetFullName(),
                                VehicleName = veh.RegNo,
                                OrdersDelivered = 0,
                                OrdersDeliveredDelayed = 0,
                                OrdersMissed = 0,
                                OrdersCancelled = 0,
                                OrdersPendingDelayed = 0,
                                OrdersPendingNotDelayed = 0,
                                TravelledMeters = 0,
                                DriverActivitySummary = new DriverActivitySummaryVM(),
                                ActiveTimeInMin = 0,
                                RewardPoints = 0,
                                StartPeriodUAT = 0,
                                EndPeriodUAT = 0,
                                CashCollected = 0,
                                DaysWorked = 0,
                            };
                            if (statusDriver != null)
                            {
                                driverOrderSummary.Dls = statusDriver.dls;
                                driverOrderSummary.Das = statusDriver.das;
                                driverOrderSummary.DlsTime = statusDriver.CreatedAt;
                                driverOrderSummary.DasTime = statusDriver.CreatedAt;
                                driverOrderSummary.DlsTimeAgo = (int)now.Subtract(driverOrderSummary.DlsTime).TotalMinutes;
                                driverOrderSummary.DasTimeAgo = (int)now.Subtract(driverOrderSummary.DasTime).TotalMinutes;
                            }
                            summaryList.Add(driverOrderSummary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Report GetDriverSummaryList Exception " + ex.ToString());
            }


            return Task.FromResult(summaryList.AsEnumerable());
        }

        

        public Task<IEnumerable<DriverOrderSummaryVM>> GetDriverCurrentStatusList(int? tenantId, int? branchId, int? driverId, int? vehicleId)
        {
            List<DriverOrderSummaryVM> summaryList = new List<DriverOrderSummaryVM>();
            DateTime now = DateMgr.GetCurrentIndiaTime();
            try
            {
                List<User> driverList = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.DRIVER).ToList();

                var orders = _db.Orders.Include(u => u.Driver).Include(u => u.Driver.Profile).Include(u => u.Vehicle).Include(u => u.Address).Where(p => p.DriverId > 0).AsQueryable();
                if (driverId != null)
                {
                    driverList = driverList.Where(p => p.Id == driverId).ToList();
                }
                if (vehicleId != null)
                {
                    Vehicle veh = _db.Vehicles.Where(p => p.Id == vehicleId).FirstOrDefault();
                    if (veh != null && veh.DriverId > 0)
                    {
                        driverList = driverList.Where(p => p.Id == veh.DriverId).ToList();
                    }
                }
                if (branchId != null)
                {
                    driverList = driverList.Where(p => p.BranchId == branchId).ToList();
                }
                if (tenantId != null)
                {
                    driverList = driverList.Where(p => p.TenantId == tenantId).ToList();
                }
                if (driverList.Count > 0)
                {
                    Dictionary<int?, List<Order>> ordersGroupedBy = orders.GroupBy(p => p.DriverId).ToDictionary(p => p.Key, p => p.ToList());
                    foreach (var driver in driverList)
                    {
                        string driverName = driver.Profile.GetFullName();
                        Vehicle veh = _db.Vehicles.Where(p => p.DriverId == driver.Id).FirstOrDefault();

                        var statusDriver = _db.DriverActivities.Where(p => p.UserId == driver.Id).OrderByDescending(x => x.Id).FirstOrDefault();

                        DriverOrderSummaryVM driverOrderSummary = new DriverOrderSummaryVM
                        {
                            DriverId = driver.Id,
                            DriverName = driverName,
                        };
                        if (veh != null)
                        {
                            driverOrderSummary.VehicleName = veh.RegNo;
                            driverOrderSummary.VehicleId = veh.Id;
                        }
                        if (statusDriver != null)
                        {
                            driverOrderSummary.Dls = statusDriver.dls;
                            driverOrderSummary.Das = statusDriver.das;
                            driverOrderSummary.DlsTime = statusDriver.CreatedAt;
                            driverOrderSummary.DasTime = statusDriver.CreatedAt;
                            driverOrderSummary.DlsTimeAgo = (int)now.Subtract(driverOrderSummary.DlsTime).TotalMinutes;
                            driverOrderSummary.DasTimeAgo = (int)now.Subtract(driverOrderSummary.DasTime).TotalMinutes;
                        }
                        summaryList.Add(driverOrderSummary);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Report GetDriverCurrentStatusList Exception " + ex.ToString());
            }

            return Task.FromResult(summaryList.AsEnumerable());
        }

        public Task<IEnumerable<DriverActivity>> GetDriverActivityList(DateTime fromDate, DateTime toDate, int tenantId, int? branchId, int? driverId, int? vehicleId)
        {
            var activities = _db.DriverActivities.Include(p => p.User.Branch).Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate).AsEnumerable();
            if (branchId != null)
            {
                activities = activities.Where(p => p.User.BranchId == branchId);
            }
            if (vehicleId != null)
            {
                activities = activities.Where(p => p.VehicleId == vehicleId);
            }
            if (driverId != null)
            {
                activities = activities.Where(p => p.UserId == driverId);
            }
            return Task.FromResult(activities.OrderBy(p => p.VehicleId).ThenByDescending(p => p.CreatedAt).AsEnumerable());
        }

        Task<DriverOrderSummaryVM> IReportQueries.GetCurrentDriverSummary(int? driverId, int? vehicleId)
        {
            var statusDriver = _db.DriverActivities.Where(p => p.UserId == driverId).OrderByDescending(x => x.Id).FirstOrDefault();

            if (statusDriver != null)
            {
                DriverOrderSummaryVM vm = new DriverOrderSummaryVM();
                vm.Das = statusDriver.das;
                vm.Dls = statusDriver.dls;

                //DriverName = driverName,
                //        OrdersDelivered = deliveredOrders,
                //        OrdersMissed = ordersMissed,
                //        TravelledMeters = 800039,
                //        ActiveTimeInMin = 2000,
                //        RewardPoints = 1560,
                //        StartPeriodUAT = 44445,
                //        EndPeriodUAT = 46667,
                //        CashCollected = 3000,
                //        DaysWorked = 1,
                //        Dls = DLoginState.LoggedIN,
                //        Das = DActivityState.BacktoJob

                return Task.FromResult(vm);
            }
            return null;
        }

        public async Task<List<OrderModel>> GetOrderSourceList(DateTime fromDate, DateTime toDate, int tenantId, int? branchId)
        {
            List<OrderModel> orderModelList = new List<OrderModel>();
            try
            {
                List<Order> orderList = _db.Orders.Include(u => u.User).Include(u => u.User.Profile).Include(u => u.Vehicle)
                    .Include(u => u.Vehicle.Driver).Include(u => u.Vehicle.Driver.Profile).Include(u => u.Address)
                    .Include(u => u.PaymentMode).Where(p => p.TenantId == tenantId && p.DeliveryFrom >= fromDate &&
                    p.DeliveryTo < toDate && p.Status != OrderStatus.ADMIN_CANCELLED && p.Status != OrderStatus.CUSTOMER_CANCELLED
                    && p.Status != OrderStatus.DRIVER_CANCELLED).ToList();
                var deliverySlotList = _db.DeliverySlots.Where(p => p.TenantId == tenantId).ToList();
                if (branchId != null)
                {
                    orderList = orderList.Where(p => p.BranchId == (int)branchId).ToList();
                    deliverySlotList = deliverySlotList.Where(p => p.BranchId == branchId).ToList();
                }
                foreach (var order in orderList)
                {
                    OrderModel orderModel  = _orderQueries.MapToOrderModel(order, deliverySlotList, false);
                    if (orderModel.Source == null)
                    {
                        orderModel.Source = Source.CUSTOMER_APP;
                    }
                        orderModelList.Add(orderModel);
                }
            }
            catch(Exception ex)
            {
                _logger.LogCritical("Report GetOrderSourceList Exception " + ex.ToString());
            }
            return orderModelList;
        }

        public async Task<TransactionSummaryVM> GetDistributorTransactionList(int month, int year, int distributorId)
        {
            TransactionSummaryVM summary = new TransactionSummaryVM();
            
            try
            {
                var orders = _db.Orders.Include(u => u.Driver).Include(u => u.Driver.Profile).Include(p => p.Distributor).Include(u => u.User).Include(u => u.User.Profile).Include(u => u.Address).Where(p => p.DistributorId == distributorId).AsQueryable();

                orders = orders.Where(p => p.DeliveredAt.Value.Date.Month == month);
                orders = orders.Where(p => p.DeliveredAt.Value.Date.Year == year);
                orders = orders.Where(p => p.Status == OrderStatus.DELIVERED);
                var orderList = orders.ToList();
                summary.DistributorId = distributorId;
                summary.Month = month;
                summary.Year = year;
                List<OrderSummaryVM> orderSummaries = new List<OrderSummaryVM>();
                int i = 1;
                foreach (Order order in orderList)
                {
                    OrderSummaryVM orderSummary = new OrderSummaryVM();
                    string invoice = _db.Invoices.Where(p => p.OrderId == order.Id).Select(p => p.InvoiceNumber).FirstOrDefault() ;
                    string invoiceCode = _db.Invoices.Where(p => p.OrderId == order.Id).Select(p => p.PdfLink).FirstOrDefault();
                    //var baseUrl = _apiSettings.Value.CustomerWebUrl;
                    //var link = baseUrl + "/Inv/V?c=";
                    var link = _apiSettings.Value.CustomerInvoiceUrl;
                    link += invoiceCode;
                    orderSummary.DriverName = order.Driver.Profile.FirstName+" "+order.Driver.Profile.LastName;
                    orderSummary.CustomerName = order.User.Profile.FirstName+" "+order.User.Profile.LastName;
                    string address = "";
                    if (!String.IsNullOrEmpty(order.Address.BuildingNo))
                    {
                        address += order.Address.BuildingNo + ", ";
                    }
                    address += order.Address.Location;
                   
                    
                    if (order.Address.PinCode > 0)
                    {
                        address += ", " + order.Address.PinCode.ToString();
                    }
                    orderSummary.SlNo = i;
                    orderSummary.InvoiceNumber = invoice ?? "Not Available";
                    orderSummary.Address = address;
                    orderSummary.DeliveredDate = ((DateTime)order.DeliveredAt).ToString("dd/MM/yyyy");
                    orderSummary.TotalAmount = order.TotalAmount;
                    var taxableAmount = order.TotalAmount / (1 + .05);
                    var tax = order.TotalAmount - taxableAmount;
                    orderSummary.TaxableAmount = Math.Round(taxableAmount,2);
                    if(invoice == null)
                    {
                        orderSummary.Invoices = "Not Available";
                    }
                    else
                    {
                        orderSummary.Invoices = link;
                    }
                    orderSummary.Tax = Math.Round(tax,2);
                    if (order.Type == OrderType.NC)
                    {
                        orderSummary.OrderType = "NC";
                    }
                    if (order.Type == OrderType.SPARE)
                    {
                        orderSummary.OrderType = "SPARE";
                    }
                    if (order.Type == OrderType.NC_REFILL)
                    {
                        orderSummary.OrderType = "NC_REFILL";
                    }
                    if (order.Type == OrderType.NC_SPARE)
                    {
                        orderSummary.OrderType = "NC_SPARE";
                    }
                    if (order.Type == OrderType.REFILL)
                    {
                        orderSummary.OrderType = "REFILL";
                    }
                    if (order.Type == OrderType.REFILL_SPARE)
                    {
                        orderSummary.OrderType = "REFILL_SPARE";
                    }
                    if (order.Type == OrderType.NC_REFILL_SPARE)
                    {
                        orderSummary.OrderType = "NC_REFILL_SPARE";
                    }
                    orderSummaries.Add(orderSummary);
                    i++;
                    
                }
                summary.Orders = orderSummaries;
                summary.TotalDeliveredOrders = orderList.Count();
                summary.TotalAmount = orderList.Sum(p => p.TotalAmount);
            }
            catch(Exception ex)
            {
                _logger.LogCritical("Report GetDistributorTransactionList Exception " + ex.ToString());
            }
            return summary;
        }


        */
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using Microsoft.AspNetCore.Mvc;
using Profiles.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using EasyGas.Services.Profiles.Models;
using System.Collections.Generic;
using System.Net;
using System.Globalization;

namespace EasyGas.Services.Profiles.Controllers
{
    [ApiController]
    //[Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
    [Route("api/v1/[controller]")]
    public class ReportsController : BaseApiController
    {
        
        private readonly IReportQueries _queries;
        //private readonly IOrderQueries _orderQueries;
        private readonly IEmailSender _emailSender;
        private readonly ProfilesDbContext _db;

        public ReportsController(IReportQueries queries, ICommandBus bus, IEmailSender emailSender, ProfilesDbContext db)
            : base(bus)
        {
            _queries = queries;
            //_orderQueries = orderQueries;
            _emailSender = emailSender;
            _db = db;

        }

        [HttpGet]
        [Route("driver/activity/list")]
        [ProducesResponseType(typeof(DriverActivitySummaryVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDriverActivityReport([FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] int tenantId, [FromQuery] int? branchId, [FromQuery] int? driverId)
        {
            DateTime fromDt = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime toDt = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var response = await _queries.GetDriverActivityReport(fromDt.Date, toDt.Date.AddSeconds(86399), tenantId, branchId, driverId);
            return Ok(response);
        }


        /*
        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetDriverSummary(string fromDate, string toDate, int? tenantId = null, int? branchId = null, int? driverId = null, int? vehicleId = null)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime queryFromDate = DateMgr.GetCurrentIndiaTime();
            DateTime queryToDate = queryFromDate;
            try
            {
                if (!String.IsNullOrEmpty(fromDate))
                {
                    queryFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            try
            {
                if (!String.IsNullOrEmpty(toDate))
                {
                    queryToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }

            List<DriverOrderSummaryVM> summaryList = new List<DriverOrderSummaryVM>();
            summaryList = (await _queries.GetDriverSummaryList(queryFromDate.Date, queryToDate.Date.AddSeconds(86399), tenantId, branchId, driverId, vehicleId, false)).ToList();
            return Ok(summaryList);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetDriverSummaryDatewise(string fromDate, string toDate, int? tenantId = null, int? branchId = null, int? driverId = null, int? vehicleId = null)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime queryFromDate = DateMgr.GetCurrentIndiaTime();
            DateTime queryToDate = queryFromDate;
            try
            {
                if (!String.IsNullOrEmpty(fromDate))
                {
                    queryFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            try
            {
                if (!String.IsNullOrEmpty(toDate))
                {
                    queryToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }

            List<DriverOrderSummaryVM> summaryList = new List<DriverOrderSummaryVM>();
            summaryList = (await _queries.GetDriverSummaryDatewiseList(queryFromDate.Date, queryToDate.Date.AddSeconds(86399), tenantId, branchId, driverId, vehicleId, false)).ToList();
            return Ok(summaryList);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetCurrentDriverSummary(int? driverId = null, int? vehicleId = null)
        {
            DriverOrderSummaryVM summary = new DriverOrderSummaryVM();
            summary = await _queries.GetCurrentDriverSummary(driverId, vehicleId);
            return Ok(summary);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetDriverStatusList(int tenantId, int? branchId, int? driverId, int? vehicleId)
        {
            List<DriverOrderSummaryVM> statusList = (await _queries.GetDriverCurrentStatusList(tenantId, branchId, driverId, vehicleId)).ToList();
            return Ok(statusList);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetDriverActivityList(string fromDate, string toDate, int tenantId, int? branchId, int? driverId, int? vehicleId)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime queryFromDate = DateMgr.GetCurrentIndiaTime();
            DateTime queryToDate = queryFromDate;
            try
            {
                if (!String.IsNullOrEmpty(fromDate))
                {
                    queryFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            try
            {
                if (!String.IsNullOrEmpty(toDate))
                {
                    queryToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            IEnumerable<DriverActivity> activity = await _queries.GetDriverActivityList(queryFromDate, queryToDate.AddDays(1), tenantId, branchId, driverId, vehicleId);
            List<DriverActivity> activityList = activity.ToList();
            activityList.ForEach(s => s.User = null);
            return Ok(activityList);
        }

        

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetOrderSourceList(string fromDate, string toDate, int tenantId, int? branchId)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime queryFromDate = DateMgr.GetCurrentIndiaTime();
            DateTime queryToDate = queryFromDate;
            try
            {
                if (!String.IsNullOrEmpty(fromDate))
                {
                    queryFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            try
            {
                if (!String.IsNullOrEmpty(toDate))
                {
                    queryToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", provider);
                }
            }
            catch (FormatException ex) { }
            List<OrderModel> orderList = (await _queries.GetOrderSourceList(queryFromDate.Date, queryToDate.Date.AddSeconds(86399), tenantId, branchId)).ToList();
            return Ok(orderList);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetTransactionSummaryList(int distributorId, int month, int year)
        {
            var tranctionSummary = await _queries.GetDistributorTransactionList(month, year, distributorId);
            return Ok(tranctionSummary);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> SendTransactionSummaryList(int distributorId, int month, int year)
        {
            List<string> errors = new List<string>();
            try
            {
                var user = _db.Users.Include(p => p.Profile).Where(p => p.Id == distributorId && p.Type == UserType.DISTRIBUTOR).FirstOrDefault();
                var distEmail = user.Profile.Email ?? throw new Exception("No Email Found");
                var tranctionSummary = await _queries.GetDistributorTransactionList(month, year, distributorId);
                var filename = await _reportService.CreateExcel(tranctionSummary);
                var command = new UploadExcelCommand(filename);
                ProcessCommand(command);
                var email = _emailSender.SendEmailToDistributorAsync(distributorId, filename, month, year);
                return Ok();
            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
                return CommandResult.FromValidationErrors(errors.AsEnumerable());
            }
        }

        //[Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetOrdersDetailList(string fromDate, string toDate, int tenantId, int? branchId)
        {
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime queryFromDate = DateMgr.GetCurrentIndiaTime();
                DateTime queryToDate = queryFromDate;
                try
                {
                    if (!String.IsNullOrEmpty(fromDate))
                    {
                        queryFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", provider);
                    }
                }
                catch (FormatException ex) { }
                try
                {
                    if (!String.IsNullOrEmpty(toDate))
                    {
                        queryToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", provider);
                    }
                }
                catch (FormatException ex) { }

                var ordersList = (await _orderQueries.GetReportOrdersList(0, 1000, queryFromDate.Date, queryToDate.Date, null, null, null, tenantId, branchId)).ToList();
                var unapprovedordersList = (await _orderQueries.GetUnApprovedOrdersReportList(queryFromDate.Date, queryToDate.Date, 0, 1000, tenantId, branchId)).ToList();
                OrderDetailsReportVM model = new OrderDetailsReportVM()
                {
                    OrdersList = ordersList,
                    UnapprovedOrdersList = unapprovedordersList
                };
                //orderVM.UnApprovedOrdersList = (await _orderQueries.GetUnApprovedExpressOrdersList(queryFromDate.Date, queryToDate.Date.AddDays(1), from, size, tenantId, branchId)).ToList();

                return Ok(model);
            }
            catch(Exception ex)
            {

            }
            return BadRequest();
        }
        */
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Infrastructure.Services;
using Profiles.API.Models;
using Profiles.API.ViewModels.Account;
using Profiles.API.ViewModels.Analytics;
using Profiles.API.ViewModels.Complaint;
using Profiles.API.ViewModels.Crm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Profiles.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
    public class AnalyticsController : BaseApiController
    {
        private readonly ILogger<CrmController> _logger;
        private readonly IIdentityService _identityService;
        private readonly ProfilesDbContext _db;
        private readonly CrmMgr _crmMgr;
        private readonly IOptions<ApiSettings> _apiSettings;

        public AnalyticsController(ILogger<CrmController> logger, ICommandBus bus, 
            IIdentityService identityService, ProfilesDbContext db, CrmMgr crmMgr, IOptions<ApiSettings> apiSettings)
            : base(bus)
        {
            _logger = logger;
            _identityService = identityService;
            _db = db;
            _crmMgr = crmMgr;
            _apiSettings = apiSettings;
        }

        [Route("customer/count/daywise")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(List<>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CustomerOrderedRatioDaywise(string fromDate, [FromQuery] string toDate)
        {
            DateTime fromDt = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime toDt = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            toDt = toDt.AddDays(1).AddSeconds(-1);

            var customers = await _db.Profiles
                .Include(p => p.User)
                .Where(p => p.User.Type == UserType.CUSTOMER && p.CreatedAt >= fromDt && p.CreatedAt <= toDt)
                .ToListAsync();

            List<CustomerOrderAnalyticsPeriodwise> reponse = new();
            DateTime startDate = fromDt;
            while(startDate <= toDt)
            {
                DateTime endDate = startDate.Date.AddDays(1).AddSeconds(-1);
                reponse.Add(new CustomerOrderAnalyticsPeriodwise()
                {
                    FromDate = startDate.Date,
                    ToDate = endDate,
                    Analytics = new CustomerOrderAnalytics
                    {
                        CustomersRegistered = customers
                        .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                        .Count(),
                        CustomersOrdered = customers
                        .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.LastOrderDeliveredAt != null)
                        .Count(),
                    }
                });

                startDate = startDate.AddDays(1);
            }

            return Ok(reponse);
        }

        [Route("customer/count/monthwise")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(List<CustomerOrderAnalyticsPeriodwise>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CustomerOrderedRatioMonthwise([FromQuery] string fromDate, [FromQuery] string toDate)
        {
            DateTime fromDt = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            fromDt = new DateTime(fromDt.Year, fromDt.Month, 1);
            DateTime toDt = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            toDt = new DateTime(toDt.Year, toDt.Month, DateTime.DaysInMonth(toDt.Year, toDt.Month)).AddDays(1).AddSeconds(-1);

            var customers = await _db.Profiles
                .Include(p => p.User)
                .Where(p => p.User.Type == UserType.CUSTOMER && p.CreatedAt >= fromDt && p.CreatedAt <= toDt)
                .ToListAsync();

            List<CustomerOrderAnalyticsPeriodwise> reponse = new();
            DateTime startDate = fromDt;
            while (startDate <= toDt)
            {
                DateTime endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month)).AddDays(1).AddSeconds(-1);
                reponse.Add(new CustomerOrderAnalyticsPeriodwise()
                {
                    FromDate = startDate.Date,
                    ToDate = endDate,
                    Analytics = new CustomerOrderAnalytics
                    {
                        CustomersRegistered = customers
                        .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                        .Count(),
                        CustomersOrdered = customers
                        .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.LastOrderDeliveredAt != null)
                        .Count(),
                    }
                });

                startDate = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1);
            }

            return Ok(reponse);
        }

        //[AllowAnonymous]
        [Route("customer/list")]
        [HttpPost]
        [ProducesResponseType(typeof(List<CustomerBasicDetails>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSelectedCustomerList(List<string> customers)
        {
            var customerDetails = await _db.Profiles
                .Include(p => p.User)
                .Where(p => p.User.Type == UserType.CUSTOMER && customers.Contains(p.Mobile))
                .Select(p => new CustomerBasicDetails
                {
                    Id = p.UserId,
                    Name = p.FirstName + " " + p.LastName,
                    Mobile = p.Mobile,
                    LastOrderedAt = p.LastOrderDeliveredAt
                })
                .ToListAsync();

            return Ok(customerDetails);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Controllers;
using System.Globalization;

namespace EasyGas.Services.Profiles.Controllers
{
    [ApiController]
    [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
    [Route("profiles/api/v1/[controller]")]
    public class DashboardController : BaseApiController
    {
        private readonly IProfileQueries _profileQueries;
        private readonly IVehicleQueries _vehicleQueries;

        public DashboardController(IProfileQueries profileQueries, IVehicleQueries vehivleQueries, ICommandBus bus)
            : base(bus)
        {
            _profileQueries = profileQueries;
            _vehicleQueries = vehivleQueries;
        }

        [Route("home")]
        [HttpGet()]
        public async Task<IActionResult> GetPvtWebInfo([FromQuery] string date, int? tenantId, int? branchId)
        {
            //DateTime fromDt = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            PvtWebDashboardVM dashboardModel = await _profileQueries.GetCrmTicketInfoForAdminDashboard(tenantId);
            dashboardModel.Vehicles = await _vehicleQueries.GetAllList(tenantId, branchId, null);

            return Ok(dashboardModel);
        }

            /*
        [Authorize]
        [Route("{userId:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetCustomerWebInfo(int userId)
        {
            try
            {
                CustomerWebDashboardVM dashboardModel = new CustomerWebDashboardVM();
                int from = 0;
                int size = 10000; //TODO max size is hardcoded here
                dashboardModel.OrdersList = (await _orderQueries.GetOrdersList(from, size, null, null, userId, null, null, null, null)).ToList();
                dashboardModel.UnreadNotificatonsCount = _notificationMgr.GetTotalUnreadNotifications(userId);
                return Ok(dashboardModel);
            }
            catch (FormatException ex)
            {
                return BadRequest("Some internal error has occured.");
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [Route("api/[controller]/[action]")]
        [Authorize]
        [HttpPost]
        public IActionResult GetSenselVehicledetails([FromBody] TenantModelforSenselAPI tenant)
        {

            try
            {
                var details =  _vehicleService.GetVehicleDetails(tenant.Id.ToString());
                return Ok(details);
            }
            catch (FormatException ex)
            {
                return BadRequest("Some internal error has occured.");
            }
            catch (Exception ex)
            {
                return NotFound();
            }

        }
        */
    }
}

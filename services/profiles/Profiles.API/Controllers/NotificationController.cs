using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Profiles.API.Controllers;
using Profiles.API.ViewModels.Notification;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
    public class NotificationController : BaseApiController
    {
        private readonly NotificationMgr _notiMgr;
        private readonly NotificationSettingsJobMgr _notiJobMgr;

        public NotificationController(NotificationMgr notiMgr, NotificationSettingsJobMgr notiJobMgr, IConfiguration cfg, ICommandBus bus)
            : base(bus)
        {
            _notiMgr = notiMgr;
            _notiJobMgr = notiJobMgr;
        }

        [Authorize]
        [Route("{userId:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetListByUser(int userId)
        {
            try
            {
                List<Notification> notiList = _notiMgr.GetListByUser(userId);
                return Ok(notiList);
            }
            catch (Exception ex)
            {
                
            }
            return BadRequest();
        }


        [Route("run/{branchId:int}")]
        [HttpPost()]
        public async Task<IActionResult> RunJobs(int branchId)
        {
            var response = await _notiJobMgr.RunCronJob(branchId);
            return response;
        }


        [Route("settings/index")]
        [HttpGet]
        public async Task<IActionResult> GetIndexModel([FromQuery] int tenantId, [FromQuery] int? branchId)
        {
             var notiSettings = await _notiMgr.GetAllNotificationSettings(tenantId, branchId);
             return Ok(notiSettings);
        }

        [Route("settings/create")]
        [HttpPost]
        public async Task<IActionResult> CreateSettings([FromBody] NotificationSettings settings)
        {
             return await _notiMgr.CreateNotificationSettings(settings);
        }

        [Route("settings/update")]
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] NotificationSettings settings)
        {
             return await _notiMgr.UpdateNotificationSettings(settings);
        }

        [HttpPut]
        [Route("send/test")]
        public async Task<IActionResult> SendTestNotification([FromBody] NotificationSettings settings)
        {
             return await _notiMgr.SendTestNotification(settings);
        }

        [HttpPost]
        [Route("send/custom")]
        public async Task<IActionResult> SendCustomNotification([FromBody] CustomNotificationRequest request)
        {
            return await _notiMgr.SendCustomNotification(request);
        }

        [Route("report")]
        [HttpGet()]
        public async Task<IActionResult> GetListByDate([FromQuery] string date, [FromQuery] int? tenantId, [FromQuery] int? branchId)
        {
            try
            {
                DateTime notiDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                List<NotificationReport> notiList = await _notiMgr.GetListByDate(notiDate);
                return Ok(notiList);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            
        }

        [Authorize]
        [Route("templates/add")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerNotificationTemplate([FromBody] AddNotificationTemplateRequest request)
        {
            return await _notiMgr.AddCustomerNotificationTemplate(request);
        }

        [Route("templates/update")]
        [HttpPut]
        public async Task<IActionResult> UpdateCustomerNotificationTemplate([FromBody] UpdateNotificationTemplateRequest request)
        {
            return await _notiMgr.UpdateCustomerNotificationTemplate(request);
        }

        [Route("templates/list")]
        [HttpGet()]
        public async Task<IActionResult> GetActiveTemplatesListAsyc()
        {
            try
            {
                var templates = await _notiMgr.GetActiveCustomerNotificationTemplates();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("templates/get")]
        [HttpGet()]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            try
            {
                var template = await _notiMgr.GetCustomerNotificationTemplate(id);
                return Ok(template);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

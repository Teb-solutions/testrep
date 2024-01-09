using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using EasyGas.Services.Profiles.Models.AdminWebsiteVM;
using EasyGas.Services.Profiles.BizLogic;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Controllers;
using System.Net;
using Profiles.API.Infrastructure.Services;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels;
using Profiles.API.Models.DriverPickupOrder;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;
using Profiles.API.ViewModels.CartAggregate;
using Profiles.API.ViewModels.DriverPickup;
using Profiles.API.IntegrationEvents;
using EasyGas.BuildingBlocks.EventBus.Events;
using EventVehicleModel = EasyGas.BuildingBlocks.EventBus.Events.VehicleModel;
using EasyGas.Shared.Models;
using Microsoft.Extensions.Logging;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/v1/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class StaffController : BaseApiController
    {
        private readonly IProfileQueries _queries;
        private readonly IVehicleQueries _vehicleQueries;
        private readonly IBusinessEntityQueries _businessEntityQueries;
        private NotificationMgr _notiMgr;
        private WalletMgr _walletMgr;
        private VehicleMgr _vehicleMgr;
        private readonly ProfileMgr _profileMgr;
        private readonly OtpMgr _otpMgr;
        private readonly IIdentityService _identityService;
        private readonly IProfileQueries _profileQueries;
        private readonly IProfilesIntegrationEventService _profilesIntegrationEventService;
        private readonly ILogger _logger;

        public StaffController(IProfileQueries queries, NotificationMgr notiMgr, WalletMgr walletMgr,
            IIdentityService identityService, VehicleMgr vehicleMgr, IProfileQueries profileQueries,
            IBusinessEntityQueries businessEntityQueries, ProfileMgr profileMgr, OtpMgr otpMgr,
            IConfiguration cfg, ICommandBus bus, IVehicleQueries vehicleQueries, ILogger<DriversController> logger,
            IProfilesIntegrationEventService profilesIntegrationEventService)
            : base(bus)
        {
            _queries = queries;
            _notiMgr = notiMgr;
            _walletMgr = walletMgr;
            _identityService = identityService;
            _vehicleMgr = vehicleMgr;
            _profileMgr = profileMgr;
            _otpMgr = otpMgr;
            _profileQueries = profileQueries;
            _businessEntityQueries = businessEntityQueries;
            _vehicleQueries = vehicleQueries;
            _profilesIntegrationEventService = profilesIntegrationEventService;
            _logger = logger;
        }

        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordModel data)
        {
            LoginModel loginModel = new LoginModel
            {
                GrantType = LoginModel.PasswordGrantType,
                Credentials = data.Password,
                // UserType = UserType.DRIVER,
                UserName = data.UserName,
                DeviceId = data.DeviceId,
                // Source = EasyGas.Shared.Source.DRIVER_APP
            };

            return await _identityService.Authenticate(loginModel, GetIpAddress());
        }

        /*
        [HttpGet]
        [Route("profile")]
        [ProducesResponseType(typeof(DriverProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProfile()
        {
            int userId = _identityService.GetUserIdentity();
            var driver = await _profileQueries.GetDriverProfileByUserId(userId);
            if (driver == null)
            {
                return NotFound();
            }
            return Ok(driver);
        }

        [Route("profile")]
        [HttpPut]
        [ProducesResponseType(typeof(DriverProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDriverProfileModel driverProfile)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.UpdateDriverProfile(userId, driverProfile);
        }

        [Authorize]
        [HttpPut("profile/image")]
        public IActionResult UpdateProfileImage([FromBody] UpdateImageModel imageModel)
        {
            int userId = _identityService.GetUserIdentity();
            var command = new UpdateProfileImageCommand(userId, imageModel);
            return ProcessCommand(command);
        }

        [HttpDelete]
        [Route("profile/image")]
        public IActionResult DeleteProfileImage()
        {
            int userId = _identityService.GetUserIdentity();
            var command = new DeleteProfileImageCommand(userId);
            return ProcessCommand(command);
        }

     */
    }
}

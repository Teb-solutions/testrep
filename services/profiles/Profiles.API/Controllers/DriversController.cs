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
    [Route("services/profiles/api/v1/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class DriversController : BaseApiController
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

        public DriversController(IProfileQueries queries, NotificationMgr notiMgr, WalletMgr walletMgr,
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

        [AllowAnonymous]
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
                UserType = UserType.DRIVER,
                UserName = data.UserName,
                DeviceId = data.DeviceId,
                Source = EasyGas.Shared.Source.DRIVER_APP
            };

            return await _identityService.Authenticate(loginModel, GetIpAddress());
        }

        [Authorize(AuthenticationSchemes = "Cognito")]
        [HttpGet]
        [Route("cognito/profile")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCognitoProfile()
        {
            var cognitouserId = _identityService.GetCognitoUserId();
            var cognitousername = _identityService.GetCognitoUsername();
            /*
            var driver = await _profileQueries.GetDriverProfileByUserId(userId);
            if (driver == null)
            {
                return NotFound();
            }
            */
            return Ok(new {cognitouserId, cognitousername});
        }

        [Authorize(AuthenticationSchemes = "Cognito")]
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

        [HttpPut]
        [Route("updatemobile/otp")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileGetOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMobileSendOtp([FromBody] UpdateMobileGetOTPRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.UpdateDriverMobileSendOtp(userId, request);
        }

        [HttpPut]
        [Route("updatemobile/otp/resend")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileGetOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LoginByOTPResponse>> UpdateMobileReSendOtp([FromBody] LoginByOTPResendRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.UpdateDriverMobileReSendOtp(userId, request);
        }

        [HttpPut]
        [Route("updatemobile/otp/validate")]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileValidateOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMobileValidateOtp([FromBody] UpdateMobileValidateOTPRequest request)
        {
            int userId = _identityService.GetUserIdentity();

            bool otpValid = _otpMgr.ValidateOTP(request.OTPUniqueId, request.OTPValue, request.Mobile);
            if (otpValid)
            {
                UserAndProfileModel userProfileModel = await _profileQueries.GetByUserId(userId);

                userProfileModel.Mobile = request.Mobile;
                var command = new UpdateMobileCommand(userProfileModel);
                return ProcessCommand(command);
            }
            else
            {
                return CommandResult.FromValidationErrors("Invalid OTP");
            }
        }

        [Authorize]
        [Route("app/config")]
        [HttpGet()]
        [ProducesResponseType(typeof(DriverAppMasterVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAppConfig()
        {
            int userId = _identityService.GetUserIdentity();
            var response = await _queries.GetDriverAppConfig(userId);
            return response;
        }

        [Authorize]
        [Route("activity")]
        [HttpGet()]
        [ProducesResponseType(typeof(DriverActivityResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetActivityStatus()
        {
            int userId = _identityService.GetUserIdentity();
            return await _vehicleMgr.GetActivityStatus(userId);
        }

        [Authorize]
        [Route("activity")]
        [HttpPut()]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateActivityStatus([FromBody] DriverActivityRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            _logger.LogInformation("Driver UpdateActivityStatus for {userId} and {@req}", userId, request);

            var result = await _vehicleMgr.DriverOnStateChange(userId, 
                request.DriverLoginState, 
                request.DriverActivityState,
                request.VehicleLat, request.VehicleLng);

            if (result.IsOk)
            {
                if (result.Content is Vehicle)
                {
                    /*
                    var veh = result.Content as Vehicle;
                    
                    VehiclePlanningStartedIntegrationEvent @planningEvent = new VehiclePlanningStartedIntegrationEvent()
                    {
                        BranchId = veh.BranchId,
                        TenantId = veh.TenantId,
                        Vehicles = await _vehicleQueries.GetAllListForPlanning(veh.TenantId, veh.BranchId, null, true)
                    };

                    //auto planning on driver state change commented as drivers are simply clicking break/resume to refresh orders
                    await _profilesIntegrationEventService.PublishEventThroughEventBusAsync(@planningEvent);
                    */
                }
                return Ok(new ApiResponse("Status updated successfully"));
            }

            return result;
        }

        [Authorize]
        [Route("details/pickup")]
        [HttpGet()]
        [ProducesResponseType(typeof(DriverPickupOrderDetailsViewModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPickupOrderDetailsForBooking([FromQuery] int driverId, [FromQuery] int? relaypointId)
        {
            int userId = _identityService.GetUserIdentity();

            DriverPickupOrderDetailsViewModel response = new DriverPickupOrderDetailsViewModel()
            { Relaypoint = new BusinessEntityModel() };

            response.Profile = await _profileQueries.GetDriverProfileByUserId(driverId);
            relaypointId = relaypointId == null ? response.Profile.BusinessEntityId : relaypointId;
            if (relaypointId != null)
            {
                response.Relaypoint = await _businessEntityQueries.GetDetailsById((int)relaypointId);
            }
            
            return Ok(response);
        }

        [Authorize]
        [Route("relaypoints/{orderId}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DriverPickupRelaypointDetails>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRelaypointsForPickupAsync(int orderId, [FromQuery] double? originLat, [FromQuery] double? originLng, [FromQuery] double? radius)
        {
            int userId = _identityService.GetUserIdentity();
            var relaypoints = await _businessEntityQueries.GetRelaypointListForDriverPickup(userId, orderId, originLat, originLng, radius);
            return Ok(relaypoints);
        }

        // this is used cutomer app also in my cylinders page to get details of relaypoint
        [Authorize] //role driver and customer
        [Route("relaypoint/{relaypointId}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DriverPickupRelaypointDetails>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRelaypointDetailsForPickupAsync(int relaypointId)
        {
            //int userId = _identityService.GetUserIdentity();
            var relaypoint = await _businessEntityQueries.GetRelaypointDetailsForDriverPickup(relaypointId);
            if (relaypoint != null)
            {
                return Ok(relaypoint);
            }
            else return NotFound("Relaypoint not found");
        }

        #region Admin

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("indexpage")]
        [HttpGet]
        public async Task<IActionResult> GetDriverIndexModel([FromQuery] int tenantId, [FromQuery] int? branchId, [FromQuery] int? distributorId)
        {
            PWDriverVM customerVM = new PWDriverVM();
            customerVM.DriverList = (await _queries.GetDriverList(tenantId, branchId, distributorId)).ToList();
            return Ok(customerVM);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> CreateDriverProfile([FromBody] UserAndProfileModel profile)
        {
            int userId = _identityService.GetUserIdentity();
            profile.UpdatedBy = userId.ToString();

            profile.Type = UserType.DRIVER;
            if (String.IsNullOrEmpty(profile.UserName))
            {
                profile.UserName = profile.Mobile;
            }
            if (String.IsNullOrEmpty(profile.Password))
            {
                profile.Password = profile.Mobile;
            }
            profile.AgreedTerms = true;
            var command = new CreateProfileAndUserCommand(profile);
            var commandResult = ProcessCommand(command);
            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (result.IsOk && result.Content is CreateProfileResponse)
                {
                    CreateProfileResponse profResp = result.Content as CreateProfileResponse;
                    await _walletMgr.CreateWallet(profResp.UserId);
                }
            }
            return commandResult;
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("update")]
        [HttpPut]
        public IActionResult UpdateDriverProfile([FromBody] UserAndProfileModel profile)
        {
            profile.Type = UserType.DRIVER;
            if (String.IsNullOrEmpty(profile.UserName))
            {
                profile.UserName = profile.Mobile;
            }
            if (String.IsNullOrEmpty(profile.Password))
            {
                profile.Password = profile.Mobile;
            }
            var command = new UpdateProfileCommand(profile, isPatch: false);
            return ProcessCommand(command);
        }

        [Authorize]
        [Route("{userid:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetDriverDetails(int userid)
        {
            var userWithDetailsModel = await _queries.GetDetailsByUserId(userid);
            if (userWithDetailsModel != null)
            {
                return Ok(userWithDetailsModel);
            }

            return NotFound();
        }

        #endregion
    }
}

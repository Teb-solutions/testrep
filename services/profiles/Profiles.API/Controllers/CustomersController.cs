using EasyGas.Shared;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profiles.API.Infrastructure.Services;
using Profiles.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Profiles.API.ViewModels.CartAggregate;
using EasyGas.Shared.Enums;
using EasyGas.Services.Profiles.Data;
using Profiles.API.ViewModels.Complaint;
using Profiles.API.Models;
using EasyGas.Services.Profiles.Services;
using Microsoft.Extensions.Options;
using Profiles.API.ViewModels.Wallet;
using Profiles.API.Services;
using Profiles.API.ViewModels.Relaypoint;
using EasyGas.Services.Profiles.Models.AdminWebsiteVM;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using AutoMapper;

namespace Profiles.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CustomersController : BaseApiController
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly IProfileQueries _profileQueries;
        private readonly IBusinessEntityQueries _businessEntityQueries;
        private readonly OtpMgr _otpMgr;
        private readonly WalletMgr _walletMgr;
        private readonly ProfileMgr _profileMgr;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly IIdentityService _identityService;
        private readonly IWalletService _walletService;
        private readonly GeoFenceMgr _geoFenceMgr;

        public CustomersController(ILogger<CustomersController> logger, IIdentityService identityService, ICommandBus bus, IConfiguration cfg,
            WalletMgr walletMgr, IProfileQueries profileQueries, OtpMgr otpMgr, 
            ProfileMgr profileMgr, IEmailSender emailSender,
            IWalletService walletService, IBusinessEntityQueries businessEntityQueries, GeoFenceMgr geoFenceMgr,
            IOptions<ApiSettings> apiSettings)
            : base(bus)
        {
            _profileQueries = profileQueries;
            _businessEntityQueries = businessEntityQueries;
            _otpMgr = otpMgr;
            _walletMgr = walletMgr;
            _profileMgr = profileMgr;
            _geoFenceMgr = geoFenceMgr;
            _logger = logger;
            _emailSender = emailSender;
            _identityService = identityService;
            _walletService = walletService;
            _apiSettings = apiSettings;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login/otp")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LoginByOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LoginByOTPResponse>> LoginRegisterSendOtp(
            [FromBody] LoginByOTPRequest request,
            [FromHeader(Name = "x-clientapp")] string clientapp,
            [FromQuery(Name = "lat")] double? lat = null,
            [FromQuery(Name = "lng")] double? lng = null)
        {
            //TODO get tenantId from app
            int tenantId = _apiSettings.Value.TenantIdDefault;

            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }

            UserAndProfileModel userProfileModel = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.CUSTOMER, tenantId);

            LoginByOTPResponse response = new LoginByOTPResponse()
            {
                Mobile = request.Mobile,
            };

            // if not registered
            if (userProfileModel.UserId <= 0)
            {
                UserAndProfileModel profile = new UserAndProfileModel();
                profile.Mobile = request.Mobile;
                profile.TenantId = tenantId;
                profile.UserName = profile.Mobile;
                profile.AgreedTerms = true;
                profile.Type = UserType.CUSTOMER;
                profile.DeviceId = request.DeviceId;
                profile.RegisteredFromLat = lat;
                profile.RegisteredFromLng = lng;
                profile.Source = Source.CUSTOMER_APP;
                var command = new CreateProfileAndUserCommand(profile, false, false);
                var resultCommand = ProcessCommand(command);
                if (resultCommand is CommandResult)
                {
                    var result = resultCommand as CommandResult;
                    if (result.IsOk)
                    {
                        var createProfileResponse = result.Content as CreateProfileResponse;
                        Otp otpModel = await _otpMgr.SendCustomerRegistrationOtp(profile);
                        if (otpModel.Id > 0)
                        {
                            response.IsNewUser = true;
                            response.OTPUniqueId = otpModel.UniqueId;
                            return Ok(response);
                        }
                    }
                }
            }
            else
            {
                Otp otpModel = await _otpMgr.SendLoginOtp(userProfileModel);
                if (otpModel.Id > 0)
                {
                    response.IsNewUser = !userProfileModel.OtpValidated;
                    response.IsNewUser = !userProfileModel.OtpValidated;
                    response.OTPUniqueId = otpModel.UniqueId;

                    await _profileMgr.UpdateDeviceId(new UpdateDeviceIdModel()
                    {
                        UserId = userProfileModel.UserId,
                        NewDeviceId = request.DeviceId,
                        Source = Source.CUSTOMER_APP //TODO get from headers
                    });

                    return Ok(response);
                }
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login/otp/resend")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(LoginByOTPResendResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LoginByOTPResponse>> LoginReSendOtp([FromBody] LoginByOTPResendRequest request)
        {
            //TODO get tenantId from app
            int tenantId = _apiSettings.Value.TenantIdDefault;

            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }

            UserAndProfileModel userProfileModel = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.CUSTOMER, tenantId);

            LoginByOTPResendResponse response = new LoginByOTPResendResponse()
            {
                Mobile = request.Mobile,
            };

            Otp otpModel = await _otpMgr.ReSendLoginOtp(userProfileModel, request.OTPUniqueId);
            if (otpModel.Id > 0)
            {
                response.OTPUniqueId = otpModel.UniqueId;
                return Ok(response);
            }
            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login/otp/validate")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginRegisterValidateOtp([FromBody] LoginValidateOTPRequest request)
        {
            //TODO get tenantId from app
            int tenantId = _apiSettings.Value.TenantIdDefault;

            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }

            UserAndProfileModel userProfile = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.CUSTOMER, tenantId);

            bool otpValid = _otpMgr.ValidateOTP(request.OTPUniqueId, request.OTPValue, request.Mobile);
            if (otpValid)
            {
                if (!userProfile.OtpValidated) // new registration
                {
                    userProfile.ReferralCode = request.ReferralCode;
                    userProfile.AgreedTerms = true;
                    userProfile.OtpValidated = true;
                    userProfile.OtpValidatedBy = "SELF";
                    userProfile.OtpValidatedAt = DateMgr.GetCurrentIndiaTime();

                    var updateProfileCommand = new UpdateProfileCommand(userProfile, true);
                    var updateProfileCommandResult = ProcessCommand(updateProfileCommand);
                    if (updateProfileCommandResult is CommandResult)
                    {
                        var result = updateProfileCommandResult as CommandResult;
                        if (!result.IsOk)
                        {
                            return updateProfileCommandResult;
                        }

                        // TODO convert to event publish
                        await _walletMgr.CreateCustomerWalletWithReferral(userProfile.UserId);
                    }
                }

                LoginModel loginModel = new LoginModel
                {
                    GrantType = LoginModel.OtpGrantType,
                    Credentials = "",
                    UserType = userProfile.Type,
                    UserName = userProfile.UserName,
                    DeviceId = userProfile.DeviceId,
                    Source = userProfile.Source
                };
                return await _identityService.Authenticate(loginModel, GetIpAddress(), request.TempUserToken);
            }
            else
            {
                return CommandResult.FromValidationErrors("Invalid OTP");
            }
        }

        [HttpPut]
        [Route("updatemobile/otp")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileGetOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMobileSendOtp([FromBody] UpdateMobileGetOTPRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }
            return await _profileMgr.UpdateCustomerMobileSendOtp(userId, request);
        }

        [HttpPut]
        [Route("updatemobile/otp/resend")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileGetOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<LoginByOTPResponse>> UpdateMobileReSendOtp([FromBody] LoginByOTPResendRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }
            return await _profileMgr.UpdateCustomerMobileReSendOtp(userId, request);
        }

        [HttpPut]
        [Route("updatemobile/otp/validate")]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UpdateMobileValidateOTPResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMobileValidateOtp([FromBody] UpdateMobileValidateOTPRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }

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

        [HttpGet]
        [Route("profile")]
        [ProducesResponseType(typeof(CustomerProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProfile()
        {
            int userId = _identityService.GetUserIdentity();
            var customer = await _profileQueries.GetCustomerProfileByUserId(userId);
            return Ok(customer);
        }

        [Route("profile")]
        [HttpPut]
        [ProducesResponseType(typeof(CustomerProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileModel customerProfile)
        {
            int userId = _identityService.GetUserIdentity();

            return await _profileMgr.UpdateCustomerProfile(userId, customerProfile);
        }

        [HttpPut("profileimage")]
        public IActionResult UpdateProfileImage([FromBody] UpdateImageModel imageModel)
        {
            int userId = _identityService.GetUserIdentity();
            var command = new UpdateProfileImageCommand(userId, imageModel);
            return ProcessCommand(command);
        }

        [HttpDelete]
        [Route("profileimage")]
        public IActionResult DeleteProfileImage()
        {
            int userId = _identityService.GetUserIdentity();
            var command = new DeleteProfileImageCommand(userId);
            return ProcessCommand(command);
        }

        [AllowAnonymous]
        [Route("app/config")]
        [HttpGet]
        [ProducesResponseType(typeof(CustomerAppConfig), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAppConfig([FromQuery] string deviceId, [FromQuery] int? cityId = null)
        {
            //int userId = _identityService.GetUserIdentity();

            var appconfig = await _profileQueries.GetCustomerAppConfig(cityId);

            await _profileMgr.UpdateDeviceId(new UpdateDeviceIdModel()
            {
                NewDeviceId = deviceId,
                Source = Source.CUSTOMER_APP
            });

            return Ok(appconfig);
        }

        [AllowAnonymous]
        [Route("city/pincode/{pincode}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        public IActionResult GetCityByPincode(string pincode)
        {
            Branch branch = _geoFenceMgr.GetBranchByPincode(pincode);
            if (branch == null)
            {
                return NotFound();
            }
            return Ok(new BranchModel { Id = branch.Id, Name = branch.Name });
        }

        [AllowAnonymous]
        [Route("city")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCityByLocation([FromQuery] string location)
        {
            Branch branch = await _geoFenceMgr.GetBranchByLocation(_apiSettings.Value.TenantIdDefault, location, null, null);
            if (branch == null)
            {
                return NotFound("EasyGas service is unavailable in this area. Please contact customer care if you need any help.");
            }
            return Ok(new BranchModel { Id = branch.Id, Name = branch.Name });
        }

        [AllowAnonymous]
        [Route("city/lat/{lat}/lng/{lng}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCityByLatLng(double lat, double lng)
        {
            Branch branch = await _geoFenceMgr.GetBranchByLatLng(lat, lng);
            if (branch == null)
            {
                return NotFound();
            }

            return Ok(new BranchModel { Id = branch.Id, Name = branch.Name });
        }

        [Route("city/pincode/{pincode}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetDefaultCityByPincode(string pincode)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.SetBranchByPincode(userId, pincode);
        }

        [Route("city/{id:int}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BranchModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetDefaultCityById(int id)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.SetBranch(userId, id);
        }

        [Route("cartdetails/delivery/{deliverySlotId:int}/{deliveryAddressId:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(CheckoutDeliveryOrderDetailsViewModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCheckoutCartDeliveryDetails(int deliverySlotId, int deliveryAddressId)
        {
            int userId = _identityService.GetUserIdentity();

            CheckoutDeliveryOrderDetailsViewModel response = new CheckoutDeliveryOrderDetailsViewModel() 
            { DeliveryAddress = new AddressModel(), DeliverySlot = new DeliverySlotModel() };

            response.Profile = await _profileQueries.GetCustomerProfileByUserId(userId);

            var address = await _profileQueries.GetUserAddress(deliveryAddressId, userId);
            if (address != null)
            {
                response.DeliveryAddress = address;
            }
            var slot = await _profileQueries.GetDeliverySlot(deliverySlotId);
            if (slot != null)
            {
                response.DeliverySlot = slot;
            }

            return Ok(response);
        }

        [Route("cartdetails/pickup/{relaypointId:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(CheckoutPickupOrderDetailsViewModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCheckoutCartPickupDetails(int relaypointId)
        {
            int userId = _identityService.GetUserIdentity();

            CheckoutPickupOrderDetailsViewModel response = new CheckoutPickupOrderDetailsViewModel()
            { Relaypoint = new BusinessEntityModel()};

            response.Profile = await _profileQueries.GetCustomerProfileByUserId(userId);
            var relaypoint = await _businessEntityQueries.GetDetailsById(relaypointId);
            if (relaypoint != null)
            {
                response.Relaypoint = relaypoint;
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("tickets")]
        [ProducesResponseType(typeof(IEnumerable<CustomerComplaintModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTickets()
        {
            int userId = _identityService.GetUserIdentity();

            var list = await _profileQueries.GetCustomerTickets(userId);

            return Ok(list);
        }

        [Route("tickets")]
        [HttpPost]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostTicket([FromBody] NewCustomerComplaint data)
        {
            int userId = _identityService.GetUserIdentity();
            var command = new AddComplaintCommand(userId, userId, data);
            var resultCommand = ProcessCommand(command);
            if (resultCommand is CommandResult)
            {
                var result = resultCommand as CommandResult;
                if (result.IsOk)
                {
                    var createComplaintResponse = result.Content as CreateComplaintResponse;
                    await _emailSender.SendEmailToAdminForNewComplaint(createComplaintResponse.Id);

                    return Ok("Ticket submitted successfully");
                }
                else
                {
                    return result;
                }
            }

            return BadRequest();
        }

        [Route("rewards")]
        [HttpGet]
        [ProducesResponseType(typeof(CustomerRewardsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRewards()
        {
            int userId = _identityService.GetUserIdentity();

            var activeReferralCouponTask = _walletService.GetActiveReferralCoupon();
            var couponSummaryTask = _walletService.GetCouponSummary(userId);

            var profile = await _profileQueries.GetCustomerProfileByUserId(userId);
            var referralCode = profile?.MyReferralCode.ToUpper();

            string offerTitle = "";
            string offerSummary = "";
            string shareTitle = "Hey, I use EasyGas to order my LPG cylinder.";
            string shareSummary = "Hey, I use EasyGas to order my LPG cylinder." +
                " Really convenient and a lot safe than a local reseller! Give it a try with my coupon code " + referralCode + " ." +
                " Click here https://www.easygas.co.in/ to download the App";
            string offerRules = "1. Your referral amount gets allotted immediately when your friend creates an account with EasyGas. For this, he/she should use your code during registration process. " +
                                "Once he/she places an order for a new connection with EasyGas, your allotted pointed becomes redeemable. <br/> " +
                                "2. Points earned on registration can only be used for getting a new connection. <br/>" +
                "3. TotalEnergies reserve right to update the reward structure in future. <br/>" +
                "4. Any unused active points will be lapsed after 365 days.";

            CouponModel activeReferralCoupon = await activeReferralCouponTask;
            if (activeReferralCoupon != null)
            {
                offerSummary = "On every referal your friend earns points worth " + activeReferralCoupon.ReferreeAmount + " Rs and you earns points worth " + activeReferralCoupon.ReferrerAmount + " Rs";
                offerTitle = "Refer and Earn " + activeReferralCoupon.ReferrerAmount + " Rs";
                shareTitle = "Get " + activeReferralCoupon.ReferreeAmount + " Rs off on your LPG order.";
                shareSummary = "Hey, I use EasyGas to order my LPG cylinder." +
                " Really convenient and a lot safe than a local reseller! Give it a try with my coupon code " + referralCode +
                " and get " + activeReferralCoupon.ReferreeAmount + " Rs off on your first LPG new cylinder." +
                " Click here https://www.easygas.co.in/ to download the App";
            }

            CustomerRewardsModel rewardsResponse = new CustomerRewardsModel()
            {
                MyReferralCode = referralCode,
                OfferTitle = offerTitle,
                OfferSummary = offerSummary,
                OfferRules = offerRules,
                ShareTitle = shareTitle,
                ShareSummary = shareSummary
            };

            var couponSummary = await couponSummaryTask;
            if (couponSummary != null)
            {
                rewardsResponse.ActivePoints = (int)couponSummary.Redeem;
                rewardsResponse.AllotedPoints = (int)couponSummary.Alloted;
                rewardsResponse.UsedPoints = (int)couponSummary.Used;
            }

            return Ok(rewardsResponse);
        }

        [Route("relaypoints/branch/{branchId}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<CartRelaypoint>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRelaypointsByBranchIdAsync(int branchId, [FromQuery] double? originLat, [FromQuery] double? originLng, [FromQuery] double? radius)
        {
            int userId = _identityService.GetUserIdentity();
            _logger.LogInformation("Query customers/relaypoints/branch/{branchId} , {originLat}, {originLng}, {radius}, {userId}", branchId, originLat, originLng, radius, userId);

            var relaypoints = await _businessEntityQueries.GetRelaypointListForCustomerCart(branchId, originLat, originLng, radius);
            return Ok(relaypoints);
        }

        [Route("test/requestheaders")]
        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public IActionResult TestHeaders([FromHeader(Name = "x-clientapp-version")] string clientappVersion,
            [FromHeader(Name = "x-clientapp-name")] string clientappName, [FromHeader(Name = "x-clientapp-os")] string clientappOs,
            [FromHeader(Name = "x-clientapp-os-version")] string clientappOsVersion, [FromHeader(Name = "x-clientapp-env")] string clientappEnv  )
        {
            return Ok(new { clientappVersion, clientappName, clientappOs, clientappOsVersion, clientappEnv });
        }

        #region Admin

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("indexpage")]
        [HttpGet]
        [ProducesResponseType(typeof(PWCustomerVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerIndexModel([FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] int tenantId, [FromQuery] int? branchId)
        {
            DateTime fromDt = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime toDt = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            PWCustomerVM customerVM = new PWCustomerVM();
            customerVM.CustomerList = (await _profileQueries.GetCustomersList(fromDt, toDt, tenantId, branchId)).ToList();
            //customerVM.InstalledNotRegisteredCustomers = await _profileQueries.GetInstalledNotRegisteredCustomerCount(tenantId, branchId);
            customerVM.DistributorList = await _businessEntityQueries.GetAllList(BusinessEntityType.Distributor, true, tenantId, null, null, null, null);
            return Ok(customerVM);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN, DISTRIBUTOR_ADMIN, DEALER_ADMIN")]
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomerProfile([FromBody] UserAndProfileModel profile)
        {
            int userId = _identityService.GetUserIdentity();
            profile.UpdatedBy = userId.ToString();

            profile.Type = UserType.CUSTOMER;
            if (String.IsNullOrEmpty(profile.UserName))
            {
                profile.UserName = profile.Mobile;
            }
            if (String.IsNullOrEmpty(profile.Password))
            {
                profile.Password = profile.Mobile;
            }
            profile.AgreedTerms = true;
            var command = new CreateProfileAndUserCommand(profile, false, false);
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

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("update")]
        [HttpPut]
        public IActionResult UpdateCustomerProfile([FromBody] UserAndProfileModel profile)
        {
            profile.Type = UserType.CUSTOMER;
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

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("{userid:int}")]
        [HttpGet()]
        [ProducesResponseType(typeof(UserDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerDetails(int userid)
        {
            var userWithDetailsModel = await _profileQueries.GetDetailsByUserId(userid);
            if (userWithDetailsModel != null)
            {
                return Ok(userWithDetailsModel);
            }

            return NotFound();
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("ambassador")]
        [HttpPut]
        public async Task<IActionResult> MarkAsAmbassador([FromBody] MarkAmbassadorModel profile)
        {
            return await _profileMgr.MaskAsAmbassador(profile);
        }

        
        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("validate")]
        [HttpPut]
        public async Task<IActionResult> MarkProfileValidated([FromBody] ValidateProfileModel request)
        {
            return await _profileMgr.MarkProfileValidated(request);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
        [Route("attach/distributor")]
        [HttpPost]
        public async Task<IActionResult> AttachDistributorToCustomer([FromBody] AttachDistributorModel request)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.AttachDistributorToCustomer(request, userId);
        }

        #endregion

        #region Marshall

        [AllowAnonymous]
        [HttpPost]
        [Route("createifnotexist")]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UserAndProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCustomerIfNotExist([FromBody] CreateCustomerRequest request)
        {
            //TODO get tenantId from app
            int tenantId = _apiSettings.Value.TenantIdDefault;

            if (request.Mobile.Length != 10)
            {
                return CommandResult.FromValidationErrors("Mobile number is invalid");
            }

            UserAndProfileModel userProfile = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.CUSTOMER, tenantId);
            if (userProfile == null && userProfile.UserId > 0)
            {
                return Ok(userProfile);
            }

            userProfile.Mobile = request.Mobile;
            userProfile.FirstName = request.Name;
            userProfile.Password = request.Mobile;
            userProfile.AgreedTerms = true;
            userProfile.UserName = request.Mobile;
            userProfile.TenantId = tenantId;
            userProfile.Type = UserType.CUSTOMER;
            userProfile.OtpValidated = false;

            var command = new CreateProfileAndUserCommand(userProfile, false, false);
            var commandResult = ProcessCommand(command);
            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (result.IsOk && result.Content is CreateProfileResponse)
                {
                    CreateProfileResponse profResp = result.Content as CreateProfileResponse;
                    await _walletMgr.CreateWallet(profResp.UserId);
                    userProfile.UserId = profResp.UserId;
                    return Ok(userProfile);
                }
            }

            return commandResult;
        }

        [AllowAnonymous] // todo
        [Route("details/{code}")]
        [HttpGet()]
        [ProducesResponseType(typeof(UserAndProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerByCode(string code)
        {
            var userWithDetailsModel = await _profileQueries.GetByCode(code);
            if (userWithDetailsModel != null)
            {
                return Ok(userWithDetailsModel);
            }

            return NotFound();
        }

        #endregion
    }
}

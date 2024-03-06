using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Infrastructure.Services;
using Profiles.API.ViewModels.AppImage;
using Profiles.API.BizLogic;
using EasyGas.Services.Profiles.BizLogic;
using Profiles.API.ViewModels.PulzConnect;

namespace Profiles.API.Controllers
{
    [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
    [Route("profiles/api/v1/[controller]")]
    public class ProfilesController : BaseApiController
    {
        private readonly IProfileQueries _queries;
        private readonly IIdentityService _identityService;
        private readonly SettingsMgr _settingsMgr;
        private readonly PulzConnectMgr _pulzConnectMgr;

        public ProfilesController(IProfileQueries queries, SettingsMgr settingsMgr,
            PulzConnectMgr pulzConnectMgr, IIdentityService identityService,
            ICommandBus bus)
            : base(bus)
        {
            _queries = queries;
            _identityService = identityService;
            _settingsMgr = settingsMgr;
            _pulzConnectMgr = pulzConnectMgr;
        }

        [Route("admin/createorder/details")]
        [HttpGet()]
        [ProducesResponseType(typeof(PWCreateOrderVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCreateOrderPageDetailsForAdmin([FromQuery] int tenantId, [FromQuery] int branchId)
        {
            PWCreateOrderVM response = new PWCreateOrderVM();
            response.DeliverySlotList = await _queries.GetDeliverySlotList(branchId);

            return Ok(response);
        }

        [HttpGet]
        [Route("customers/search/{name}")]
        [ProducesResponseType(typeof(IEnumerable<UserDetailsModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string name, [FromQuery] int from = 0, [FromQuery] int size = 20, int? tenantId = null, int? branchId = null)
        {
            var customers = await _queries.GetCustomersBySearch(name, from, size, tenantId, branchId);
            return Ok(customers);
        }

        [Authorize]
        [Route("{userid:int}")]
        [HttpGet()]
        public IActionResult GetProfileByUserId(int userid)
        {
            var profile = _queries.GetByUserId(userid);
            if (profile != null)
            {
                return Ok(profile);
            }

            return NotFound();
        }

        [Authorize]
        [Route("details/{userid:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetProfileDetails(int userid)
        {
            var userWithDetailsModel = await _queries.GetDetailsByUserId(userid);
            if (userWithDetailsModel != null)
            {
                return Ok(userWithDetailsModel);
            }

            return NotFound();
        }

        [Authorize]
        [Route("detailswithorder/{userid:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetProfileDetailsWithOrders(int userid)
        {
            var userWithDetailsModel = await _queries.GetDetailsByUserId(userid);
            if (userWithDetailsModel != null)
            {
                return Ok(userWithDetailsModel);
            }

            return NotFound();
        }

        [Authorize]
        [Route("addresses/{userId:int}")]
        [HttpGet()]
        public async Task<IActionResult> GetUserAddressList(int userId)
        {
            var addresses = await _queries.GetUserAddressList(userId);
            //if (addresses != null)
            {
                return Ok(addresses);
            }

            // return NotFound();
        }


        [Authorize]
        [HttpPost]
        public IActionResult CreateProfile([FromBody] UserAndProfileModel profile)
        {
            int userId = _identityService.GetUserIdentity();
            profile.UpdatedBy = userId.ToString();

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

            return ProcessCommand(command);

        }

        [Authorize]
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UserAndProfileModel profile)
        {
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
        [HttpPost]
        [Route("address")]
        public IActionResult CreateAddress([FromBody] UserAndAddressModel address)
        {
            var command = new CreateUserAddressCommand(address);
            return ProcessCommand(command);
        }

        [Authorize]
        [HttpPut]
        [Route("address")]
        public IActionResult UpdateAddress([FromBody] UserAndAddressModel address)
        {
            var command = new CreateUserAddressCommand(address);
            return ProcessCommand(command);
        }

        [Authorize]
        [Route("address/{userAddressId:int}")]
        [HttpDelete]
        public IActionResult DeleteAddress(int userAddressId)
        {
            var userId = _identityService.GetUserIdentity();
            var command = new DeleteUserAddressCommand(userAddressId, userId);
            return ProcessCommand(command);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("appimage")]
        [HttpGet]
        public async Task<IActionResult> GetAppImages([FromQuery] int? tenantId, [FromQuery] int? branchId)
        {
            return Ok(await _queries.GetAppImages(tenantId, branchId));
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("appimage")]
        [HttpPost]
        public async Task<IActionResult> AddAppImage([FromBody] AddAppImageRequest request)
        {
            var userId = _identityService.GetUserIdentity();
            return await _settingsMgr.AddAppImage(request, userId);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("appimage/{id}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveAppImage(int id)
        {
            var userId = _identityService.GetUserIdentity();
            return await _settingsMgr.RemoveAppImage(id, userId);
        }

        [AllowAnonymous]
        [Route("customer/{mobile}")]
        [HttpGet()]
        [ProducesResponseType(typeof(ProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerProfileByMobile(string mobile)
        {
            var profile = await _pulzConnectMgr.GetCustomerProfileByMobile(mobile);
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        /*
            [Authorize]
            [Route("{userid:int}")] 
            [HttpDelete]
            public IActionResult DeleteProfile(int userid)
            {
                var command = new DeleteProfileCommand(userid);
                return ProcessCommand(command);
            }
        */
    }
}

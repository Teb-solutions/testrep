using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using EasyGas.Services.Profiles.Models.AdminWebsiteVM;
using Microsoft.AspNetCore.Authorization;
using EasyGas.Services.Profiles.BizLogic;
using Profiles.API.Controllers;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.Infrastructure.Services;
using Profiles.API.ViewModels.AdminWebsiteVM;
using System.Net;
using Profiles.API.ViewModels.Relaypoint;
using EasyGas.Shared;
using System.Collections.Generic;
using System.Linq;
using Profiles.API.Services;
using Profiles.API.Models;
using System;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("profiles/api/v1/[controller]")]
    public class DistributorsController : BaseApiController
    {
        private readonly IProfileQueries _queries;
        private readonly IVehicleQueries _vehQueries;
        private readonly IBusinessEntityQueries _businessEntityQueries;
        private WalletMgr _walletMgr;
        private ProfileMgr _profileMgr;
        private readonly ILocationService _locationService;
        private readonly ICatalogService _catalogService;
        private readonly IIdentityService _identityService;

        public DistributorsController(IProfileQueries queries, WalletMgr walletMgr,
            IBusinessEntityQueries businessEntityQueries,
            IVehicleQueries vehQueries,
            IIdentityService identityService,
            ICatalogService catalogService,
            ProfileMgr profileMgr,
            ILocationService locationService,
            IConfiguration cfg, ICommandBus bus)
            : base(bus)
        {
            _queries = queries;
            _businessEntityQueries = businessEntityQueries;
            _vehQueries = vehQueries;
            _identityService = identityService;
            _walletMgr = walletMgr;
            _profileMgr = profileMgr;
            _locationService = locationService;
            _catalogService = catalogService;
        }


        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// This API is moved to BusinessEntity/login
        /// </remarks>
        [Obsolete("moved to BusinessEntity/login")]
        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordModel data)
        {
            if (!ModelState.IsValid)
            {
                return CommandResult.FromValidationErrors("Username/password is invalid");
            }
            LoginModel loginModel = new LoginModel()
            {
                UserName = data.UserName,
                Credentials = data.Password,
                DeviceId = data.DeviceId,
                GrantType = LoginModel.PasswordGrantType,
                Source = Source.DISTRIBUTOR_WEB_APP,
                UserType = UserType.DISTRIBUTOR
            };
            return await _identityService.Authenticate(loginModel, GetIpAddress());
        }

        #region Backend

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("create")]
        [HttpPost]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody] CreateDistributorRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;

            request.Type = BusinessEntityType.Distributor;
            var command = new CreateDistributorCommand(request);
            var commandResult = ProcessCommand(command);

            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (!result.IsOk)
                {
                    return commandResult;
                }

                var createDistributorResponse = result.Content as CreateDistributorResponse;
                
                // TODO convert to event publish
                await _walletMgr.CreateBusinessEntityWallet(createDistributorResponse.Id);
                return commandResult;
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("update")]
        [HttpPut]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public IActionResult Update([FromBody] CreateDistributorRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;

            var command = new UpdateDistributorCommand(request);
            return ProcessCommand(command);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("indexpage")]
        [HttpGet()]
        [ProducesResponseType(typeof(DistributorIndexpageModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] int tenantId, [FromQuery] int? branchId)
        {
            var list = await _businessEntityQueries.GetAllList(BusinessEntityType.Distributor, null, tenantId, branchId, null, null, null);
            DistributorIndexpageModel response = new DistributorIndexpageModel()
            {
                DistributorList = list
            };
            return Ok(response);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("{id:int}")]
        [HttpGet()]
        [ProducesResponseType(typeof(BusinessEntityModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var distributor = await _businessEntityQueries.GetDetailsById(id);
            if (distributor != null)
            {
                //profile.PhotoUrl = $"{_avatarsUrl}user_{profile.UserId}.jpg";
                return Ok(distributor);
            }

            return NotFound();
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("details/{id:int}")]
        [HttpGet()]
        [ProducesResponseType(typeof(CreateDistributorRequest), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDetailsById(int id)
        {
            var distributor = await _businessEntityQueries.GetDetailsById(id);
            //var workingDays = await _businessEntityQueries.GetRelaypointWorkingDays(id);
            if (distributor != null)
            {
                var user = await _businessEntityQueries.GetAdminUser(distributor.Id, UserType.DISTRIBUTOR);
                CreateDistributorRequest response = new CreateDistributorRequest()
                {
                    Id = distributor.Id,
                    Name = distributor.Name,
                    MobileNumber = distributor.MobileNumber,
                    Location = distributor.Location,
                    Lat = distributor.Lat,
                    Lng = distributor.Lng,
                    Details = distributor.Details,
                    PinCode = distributor.PinCode,
                    TenantId = distributor.TenantId,
                    BranchId = distributor.BranchId,
                    Code = distributor.Code,
                    Email = distributor.Email,
                    GSTN = distributor.GSTN,
                    PAN = distributor.PAN,
                    PaymentNumber = distributor.PaymentNumber,
                    UPIQRCodeImageUrl = distributor.UPIQRCodeImageUrl,
                    AdminUserName = user?.UserName,
                    IsActive = distributor.IsActive,
                    Rating = distributor.Rating
                };

                return Ok(response);
            }

            return NotFound();
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("enable/{id:int}")]
        [HttpPut()]
        public IActionResult Enable(int id)
        {
            int userId = _identityService.GetUserIdentity();

            var command = new UpdateDistributorStatusCommand(id, true, userId);
            return ProcessCommand(command);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("disable/{id:int}")]
        [HttpPut()]
        public IActionResult Disable(int id)
        {
            int userId = _identityService.GetUserIdentity();

            var command = new UpdateDistributorStatusCommand(id, false, userId);
            return ProcessCommand(command);
        }

        #endregion

        #region Dealer

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("dealer")]
        [HttpPost]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateDealerByAdmin([FromBody] CreateDealerRequest request)
        {
            var distributor = await _businessEntityQueries.GetDetailsById(request.ParentBusinessEntityId);
            if (distributor == null)
            {
                return BadRequest("Distributor is invalid");
            }

            var command = new CreateDealerCommand(request, distributor);
            var commandResult = ProcessCommand(command);

            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (!result.IsOk)
                {
                    return commandResult;
                }

                var createDistributorResponse = result.Content as CreateDistributorResponse;

                // TODO convert to event publish
                //await _walletMgr.CreateBusinessEntityWallet(createDistributorResponse.Id, UserType.DISTRIBUTOR);
                return commandResult;
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("dealer")]
        [HttpPut]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public IActionResult UpdateDealerByAdmin([FromBody] CreateDealerRequest request)
        {
            var command = new UpdateDealerCommand(request);
            return ProcessCommand(command);
        }

        [Authorize(Roles = "DISTRIBUTOR_ADMIN")]
        [Route("dealer/create")]
        [HttpPost]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateDealerByDistributor([FromBody] CreateDealerRequest request)
        {
            int distributorId = _identityService.GetBusinessEntityId();
            var distributor = await _businessEntityQueries.GetDetailsById(distributorId);
            if (distributor == null)
            {
                return BadRequest("Distributor is invalid");
            }

            request.ParentBusinessEntityId = distributorId;
            var command = new CreateDealerCommand(request, distributor);
            var commandResult = ProcessCommand(command);

            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (!result.IsOk)
                {
                    return commandResult;
                }

                var createDistributorResponse = result.Content as CreateDistributorResponse;

                // TODO convert to event publish
                //await _walletMgr.CreateBusinessEntityWallet(createDistributorResponse.Id, UserType.DISTRIBUTOR);
                return commandResult;
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }

        [Authorize(Roles = "DISTRIBUTOR_ADMIN")]
        [Route("dealer/update")]
        [HttpPut]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public IActionResult UpdateDealerByDistributor([FromBody] CreateDealerRequest request)
        {
            int distributorId = _identityService.GetBusinessEntityId();
            request.ParentBusinessEntityId = distributorId;
            var command = new UpdateDealerCommand(request);
            return ProcessCommand(command);
        }

        #endregion
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profiles.API.Infrastructure.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EasyGas.Shared.Models;

namespace Profiles.API.Controllers
{
    [Route("profiles/api/v1/[controller]")]
    [Authorize]
    public class AddressController : BaseApiController
    {
        private readonly ILogger<AddressController> _logger;
        private readonly IProfileQueries _profileQueries;
        private readonly IIdentityService _identityService;
        private readonly ProfileMgr _profileMgr;

        public AddressController(ILogger<AddressController> logger, IIdentityService identityService,
            ICommandBus bus, IProfileQueries profileQueries, ProfileMgr profileMgr)
            : base(bus)
        {
            _profileQueries = profileQueries;
            _identityService = identityService;
            _logger = logger;
            _profileMgr = profileMgr;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AddressModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            int userId = _identityService.GetUserIdentity();

            var list = await _profileQueries.GetUserAddressList(userId);

            return Ok(list);
        }

        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(AddressModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(int id)
        {
            int userId = _identityService.GetUserIdentity();

            var address = await _profileQueries.GetUserAddress(id, userId);

            if (address == null)
            {
                return NotFound("Address not found");
            }

            return Ok(address);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CreateUserAddressResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody] AddressModel address)
        {
            var userId = _identityService.GetUserIdentity();
            UserAndAddressModel model = new UserAndAddressModel()
            {
                UserId = userId, 
                Address = address
            };
            var command = new CreateUserAddressCommand(model);
            var resultCommand = ProcessCommand(command);
            if (resultCommand is CommandResult)
            {
                var result = resultCommand as CommandResult;
                if (result.IsOk)
                {
                    var addrResp = result.Content as CreateUserAddressResponse;
                    await _profileMgr.SetBranch(userId, addrResp.BranchId);
                }
            }

            return resultCommand;
        }

        [HttpPut]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CreateUserAddressResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update([FromBody] AddressModel address)
        {
            var userId = _identityService.GetUserIdentity();
            UserAndAddressModel model = new UserAndAddressModel()
            {
                UserId = userId,
                Address = address
            };
            var command = new CreateUserAddressCommand(model);
            var resultCommand = ProcessCommand(command);
            if (resultCommand is CommandResult)
            {
                var result = resultCommand as CommandResult;
                if (result.IsOk)
                {
                    var addrResp = result.Content as CreateUserAddressResponse;
                    await _profileMgr.SetBranch(userId, addrResp.BranchId);
                }
            }

            return resultCommand;
        }

        [Route("{userAddressId:int}")]
        [HttpDelete]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
        public IActionResult Delete(int userAddressId)
        {
            var userId = _identityService.GetUserIdentity();
            var command = new DeleteUserAddressCommand(userAddressId, userId);
            return ProcessCommand(command);
        }
    }
}

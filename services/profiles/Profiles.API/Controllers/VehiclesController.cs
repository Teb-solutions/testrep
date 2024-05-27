using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Controllers;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("services/profiles/api/v1/[controller]")]
    [Authorize]
    public partial class VehiclesController : BaseApiController
    {
        private readonly IVehicleQueries _queries;
        private readonly IProfileQueries _profileQueries;
        private readonly IBusinessEntityQueries _businessEntityQueries;

        public VehiclesController(IVehicleQueries queries,
            IProfileQueries profileQueries,
            IBusinessEntityQueries businessEntityQueries,
            ICommandBus bus)
            : base(bus)
        {
            _queries = queries;
            _profileQueries = profileQueries;
            _businessEntityQueries = businessEntityQueries;
        }

        [Authorize(AuthenticationSchemes = "Cognito")]
        [HttpGet]
        public async Task<IActionResult> GetTestData([FromQuery] int? tenantId, [FromQuery] int? branchId, [FromQuery] int? businessEntityId)
        {
            return Ok(new {tenantId, branchId});
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetVehicleList([FromQuery] int? tenantId, [FromQuery] int? branchId, [FromQuery] int? businessEntityId)
        {
            var list = await _queries.GetAllList(tenantId,branchId, businessEntityId, true);
            return Ok(list);
        }

        [Authorize]
        [Route("{id:int}")]
        [HttpGet]
        public async Task<IActionResult> GetVehicleDetail(int id)
        {
            var detail = await _queries.GetDetails(id, true);
            if (detail != null)
            {
                return Ok(detail);
            }
            return NotFound();
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN")]
        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> GetCreateVehicleModel([FromQuery] int? tenantId, [FromQuery] int? branchId, [FromQuery] int? businessEntityId)
        {
            PWCreateVehicleVM createVehicleVM = new PWCreateVehicleVM()
            {
                DriverSelectList = await _profileQueries.GetDriverSelectList(tenantId, branchId, businessEntityId),
                DistributorSelectList = await _businessEntityQueries.GetSelectList(Shared.Enums.BusinessEntityType.Distributor, tenantId, branchId),
            };
            return Ok(createVehicleVM);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN")]
        [HttpGet]
        [Route("update")]
        public async Task<IActionResult> GetEditVehicleModel([FromQuery] int vehicleId, [FromQuery] int? tenantId, [FromQuery] int? branchId, [FromQuery] int? businessEntityId)
        {
            VehicleModel vehModel = await _queries.GetDetails(vehicleId);
            if (vehModel != null)
            {
                PWCreateVehicleVM createVehicleVM = new PWCreateVehicleVM()
                {
                    Vehicle = vehModel,
                    DriverSelectList = await _profileQueries.GetDriverSelectList(tenantId, branchId, businessEntityId),
                    DistributorSelectList = await _businessEntityQueries.GetSelectList(Shared.Enums.BusinessEntityType.Distributor, tenantId, branchId),
                };
                return Ok(createVehicleVM);
            }
            return NotFound();
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN")]
        [Route("create")]
        [HttpPost]
        public IActionResult CreateVehicle([FromBody] AddVehicleRequest vehicle)
        {
            var command = new CreateVehicleCommand(vehicle);
            return ProcessCommand(command);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN")]
        [Route("update")]
        [HttpPut]
        public IActionResult EditVehicle([FromBody] AddVehicleRequest vehicle)
        {
            var command = new UpdateVehicleCommand(vehicle, false);
            return ProcessCommand(command);
        }
    }
}

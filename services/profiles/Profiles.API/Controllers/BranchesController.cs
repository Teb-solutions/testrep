using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Profiles.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("profiles/api/v1/[controller]")]
    [Authorize(Roles = "TENANT_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public class BranchesController : BaseApiController
    {
        private readonly ITenantQueries _queries;
        public BranchesController(ITenantQueries queries, ICommandBus bus)
            : base(bus)
        {
            _queries = queries;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Branch), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult GetById(int id)
        {
            var branch = _queries.GetBranchById(id);
            return branch == null ? (IActionResult)NotFound() : (IActionResult)Ok(branch);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Branch>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _queries.GetAllBranchesAsync(null);
            return Ok(branches);
        }

        [HttpGet]
        [Route("tenant/{id:int}")]
        [ProducesResponseType(typeof(IEnumerable<Branch>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetByTenantId(int id)
        {
            var branches = await _queries.GetAllBranchesAsync(id);
            return Ok(branches);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Branch), (int)HttpStatusCode.OK)]
        public IActionResult CreateBranch([FromBody] Branch branch)
        {
            var command = new CreateBranchCommand(branch, false);
            return ProcessCommand(command);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Branch), (int)HttpStatusCode.OK)]
        public IActionResult UpdateBranch([FromBody] Branch branch)
        {
            var command = new CreateBranchCommand(branch, true);
            return ProcessCommand(command);
        }
    }
}

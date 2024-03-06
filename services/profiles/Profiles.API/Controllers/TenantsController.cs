using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Controllers;
using System.Net;

namespace EasyGas.Services.Profiles.Controllers
{
    [Authorize]
    [Route("profiles/api/v1/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TenantsController : BaseApiController
    {
        private readonly ITenantQueries _queries;
        public TenantsController(ITenantQueries queries, ICommandBus bus)
            : base(bus)
        {
            _queries = queries;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Tenant), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult GetById(int id)
        {
            var tenant = _queries.GetById(id);
            return tenant == null ? (IActionResult)NotFound() : Ok(tenant);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Tenant>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var tenants = await _queries.GetAllAsync();
            return Ok(tenants);
        }

    }
}

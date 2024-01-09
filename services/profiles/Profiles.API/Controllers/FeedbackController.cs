using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Profiles.API.Controllers;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FeedbackController : BaseApiController
    {
        private readonly IFeedbackQueries _queries;
        public FeedbackController(ICommandBus commandBus, IFeedbackQueries queries) : base(commandBus)
        {
            _queries = queries;
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddFeedback([FromBody] FeedbackModel feedback)
        {
            var command = new AddFeedbackCommand(feedback);
            return ProcessCommand(command);
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetFeedback(int? branchId, int tenantId, FeedbackType type)
        {
            var feedbackList = (await _queries.GetFeedbackList(branchId, tenantId, type)).ToList();
            return Ok(feedbackList);
        }
    }
}

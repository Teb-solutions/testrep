using EasyGas.Services.Core.Commands;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ICommandBus CommandBus { get; }

        protected BaseApiController(ICommandBus commandBus)
        {
            CommandBus = commandBus;
        }

        protected IActionResult ProcessCommand<T>(T command) where T : ICommand
        {
            var result = CommandBus.Send(command);
            if (result.IsOk)
            {
                CommandBus.ApplyChanges();
            }

            // Some commands need to return a new command result with additional content
            // after db is hit.
            var delayedResult = CommandBus.GetDelayedCommandResult();

            return delayedResult ?? result;

        }

        protected string GetIpAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}

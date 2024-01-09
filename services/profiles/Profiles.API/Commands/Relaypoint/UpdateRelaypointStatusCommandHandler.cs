using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateRelaypointStatusCommandHandler : ICommandHandler<UpdateRelaypointStatusCommand>
    {
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateRelaypointStatusCommandHandler(ProfilesDbContext db, ILogger<UpdateRelaypointStatusCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UpdateRelaypointStatusCommand command)
        {
            var businessEntity = _db.BusinessEntities.Find(command._businessEntityId);
            if (businessEntity == null)
            {
                return CommandHandlerResult.Error($"Relaypoint does not exists.");
            }

            businessEntity.IsActive = command._isActive;
            businessEntity.UpdatedBy = command._userId.ToString();

            _logger.LogInformation("Relaypoint Status Updated | Name: " + businessEntity.Name + " Status: " + command._isActive);

            return CommandHandlerResult.OkDelayed(this, 
                x => new CreateRelaypointResponse
                {
                    Id = businessEntity.Id,
                });
        }
    }
}

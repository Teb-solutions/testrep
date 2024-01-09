using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateRelaypointStatusCommand : CommandBase
    {
        public int _businessEntityId;
        public bool _isActive;
        public int _userId;

        public UpdateRelaypointStatusCommand(int businessEntityId, bool isActive, int userId)
        {
            _businessEntityId = businessEntityId;
            _isActive = isActive;
            _userId = userId;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (_businessEntityId <= 0)
            {
                yield return "Invalid relaypoint";
            }
        }
    }
}

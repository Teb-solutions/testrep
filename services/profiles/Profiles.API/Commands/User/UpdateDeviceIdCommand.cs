using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateDeviceIdCommand : CommandBase
    {
        public UpdateDeviceIdModel _model;

        public UpdateDeviceIdCommand(UpdateDeviceIdModel model)
        {
            _model = model;
        }
        protected override IEnumerable<string> OnValidation()
        {
            if (string.IsNullOrEmpty(_model.NewDeviceId))
            {
                yield return "Device Id is empty";
            }
        }

    }
}

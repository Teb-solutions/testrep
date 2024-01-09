using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateDeviceIdCommandHandler : ICommandHandler<UpdateDeviceIdCommand>
    {
        private readonly ProfilesDbContext _db;
        public UpdateDeviceIdCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(UpdateDeviceIdCommand command)
        {
            UpdateDeviceIdModel model = command._model;
            if (model.UserId != null && model.UserId > 0)
            {
                var userProfile = _db.Profiles.Where(x => x.UserId == model.UserId).FirstOrDefault();
                if (userProfile != null)
                {
                    userProfile.DeviceId = model.NewDeviceId;
                }
                else
                {
                    return CommandHandlerResult.Error($"User Id ({model.UserId}) does not exist.");
                }
            }

            model.UserId = model.UserId > 0 ? model.UserId : null;

            UserDevice existingOld = null;
                if (!string.IsNullOrEmpty(model.OldDeviceId))
                {
                    existingOld = _db.UserDevices.Where(p => p.FirebaseDeviceId == model.OldDeviceId).FirstOrDefault();
                }
                if (existingOld == null)
                {
                    UserDevice existingNew = _db.UserDevices.Where(p => p.FirebaseDeviceId == model.NewDeviceId).FirstOrDefault();
                    if (existingNew == null)
                    {
                        existingNew = new UserDevice();
                        existingNew.FirebaseDeviceId = model.NewDeviceId;
                        existingNew.UserId = model.UserId;
                        existingNew.Source = model.Source;
                        _db.UserDevices.Add(existingNew);
                    }
                    else
                    {
                        existingNew.UserId = model.UserId;
                        existingNew.Source = model.Source;
                    }
                }
                else
                {
                    existingOld.FirebaseDeviceId = model.NewDeviceId;
                    existingOld.UserId = model.UserId;
                    existingOld.Source = model.Source;
                }

            return CommandHandlerResult.OkDelayed(this, 
                x => model);
        }
    }
}

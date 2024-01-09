using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateMobileCommandHandler : ICommandHandler<UpdateMobileCommand>
    {
        private readonly ProfilesDbContext _db;
        public UpdateMobileCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(UpdateMobileCommand command)
        {
            var profile = command.Profile;
            var user = _db.Users.SingleOrDefault(p => p.Id == profile.UserId);
            var userProfile = _db.Profiles.SingleOrDefault(p => p.UserId == profile.UserId);
            if (user == null || userProfile == null)
            {
                return CommandHandlerResult.Error("No user found");
            }
            var existingUser = _db.Users.Any(x => x.UserName == profile.Mobile && x.Type == profile.Type && x.TenantId == profile.TenantId);
            if (existingUser)
            {
                return CommandHandlerResult.Error($"User with mobile ({profile.Mobile}) already exists.");
            }

            user.UserName = profile.Mobile;
            userProfile.Mobile = profile.Mobile;
            return CommandHandlerResult.OkDelayed(this,
                x => new UpdateMobileValidateOTPResponse
                {
                    Mobile = userProfile.Mobile,
                });
        }
    }
}

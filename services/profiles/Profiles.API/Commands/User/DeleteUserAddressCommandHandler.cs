using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class DeleteUserAddressCommandHandler : ICommandHandler<DeleteUserAddressCommand>
    {
        private readonly ProfilesDbContext _db;
        public DeleteUserAddressCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(DeleteUserAddressCommand command)
        {
            var existing = _db.Addresses.SingleOrDefault(p => p.Id == command.UserAddressId && p.UserId == command.UserId);
            if (existing != null)
            {
                _db.Addresses.Remove(existing);
                return CommandHandlerResult.OkDelayed(this, x => new ApiResponse("Address deleted successfully"));
            }
            else
            {
                return CommandHandlerResult.Error($"Sorry, address not found.");
            }
        }
    }
}

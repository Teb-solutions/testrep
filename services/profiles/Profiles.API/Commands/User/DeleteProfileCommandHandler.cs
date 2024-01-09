using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class DeleteProfileCommandHandler : ICommandHandler<DeleteProfileCommand>
    {
        private readonly ProfilesDbContext _db;
        public DeleteProfileCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(DeleteProfileCommand command)
        {
            var existing = _db.Profiles.SingleOrDefault(p => p.UserId == command.UserId);
            if (existing != null)
            {
                _db.Profiles.Remove(existing);
            }

            return new CommandHandlerResult(System.Net.HttpStatusCode.OK, new ApiResponse("Profile Image deleted successfully"));
        }
    }
}

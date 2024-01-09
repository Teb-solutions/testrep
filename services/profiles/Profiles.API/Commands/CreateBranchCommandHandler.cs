using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateBranchCommandHandler : ICommandHandler<CreateBranchCommand>
    {
        private readonly ProfilesDbContext _db;
        private CreateBranchCommand _command;
        public CreateBranchCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }

        public CommandHandlerResult Handle(CreateBranchCommand command)
        {
            _command = command;
            if (command.Branch.TenantId > 0)
            {
                var tenant = _db.Tenants.Where(p => p.Id == command.Branch.TenantId).SingleOrDefault();
                if (tenant == null)
                {
                    return CommandHandlerResult.Error($"Tenant does not exist");
                }
            }

            if (_command.IsUpdate)
            {
                var existingBranch = _db.Branches.Where(x => x.Id == _command.Branch.Id).FirstOrDefault();
                if (existingBranch == null)
                {
                    return CommandHandlerResult.Error($"Branch not found");
                }
                var duplicateName = _db.Branches.Any(x => x.Name == _command.Branch.Name && x.Id != _command.Branch.Id);
                if (duplicateName)
                {
                    return CommandHandlerResult.Error($"Branch with name already exists ({command.Branch.Name})");
                }
                existingBranch.Name = _command.Branch.Name;
                existingBranch.Mobile = _command.Branch.Mobile;
                existingBranch.Location = _command.Branch.Location;
                existingBranch.IsActive = _command.Branch.IsActive;
            }
            else
            {
                var duplicateName = _db.Branches.Any(x => x.Name == _command.Branch.Name);
                if (duplicateName)
                {
                    return CommandHandlerResult.Error($"Branch with name already exists ({command.Branch.Name})");
                }
                _db.Branches.Add(command.Branch);
            }
            return CommandHandlerResult.OkDelayed(this,
                x => command.Branch);
        }
    }
}

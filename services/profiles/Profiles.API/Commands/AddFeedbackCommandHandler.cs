using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddFeedbackCommandHandler : ICommandHandler<AddFeedbackCommand>
    {
        private readonly ProfilesDbContext _db;
        private AddFeedbackCommand _command;
        public AddFeedbackCommandHandler(ProfilesDbContext db)
        {
            _db = db;
        }
        public CommandHandlerResult Handle(AddFeedbackCommand command)
        {
            _command = command;
            
            var tenant = _db.Tenants.Where(t => t.Id == command.Feedback.TenantId).FirstOrDefault();
            if(tenant == null)
            {
                return CommandHandlerResult.Error($"Tenant is invalid.");
            }
            if(command.Feedback.BranchId != null)
            {
                var branch = _db.Branches.Where(t => t.Id == command.Feedback.BranchId).FirstOrDefault();
                if(branch == null)
                {
                    return CommandHandlerResult.Error($"Branch is invalid.");

                }
                var existing = _db.Feedbacks.Where(t => t.TenantId == command.Feedback.TenantId && t.BranchId == command.Feedback.BranchId).Any(p => p.Remarks == command.Feedback.Remarks);
                if (existing)
                {
                    return CommandHandlerResult.Error($"Feedback ({command.Feedback.Remarks}) already exists.");
                }

            }
            else
            {
                var existing = _db.Feedbacks.Where(t => t.TenantId == command.Feedback.TenantId).Any(p => p.Remarks == command.Feedback.Remarks);
                if (existing)
                {
                    return CommandHandlerResult.Error($"Feedback ({command.Feedback.Remarks}) already exists.");
                }
            }
            if (!Enum.IsDefined(typeof(FeedbackType), command.Feedback.FeedbackType))
            {
                return CommandHandlerResult.Error($"Feedback Type is invalid.");
            }
            if (_command.ValidateOnly)
            {
                return CommandHandlerResult.Ok;
            }

            _db.Feedbacks.Add(command.Feedback);
            return CommandHandlerResult.OkDelayed(this,
                x => new CreateFeedbackResponse
                {
                    FeedbackId = _command.Feedback.Id
                }); ;
        }
    }
}

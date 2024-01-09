using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddFeedbackCommand : CommandBase
    {
        private readonly FeedbackModel _model;
        public Feedback Feedback { get; }
        public bool ValidateOnly { get; }
        public AddFeedbackCommand(FeedbackModel model, bool validateOnly = false)
        {
            _model = model;
            Feedback = CreateFeedbackFromModel(model);
            ValidateOnly = validateOnly;
        }
        private Feedback CreateFeedbackFromModel(FeedbackModel model)
        {
            var feedback = new Feedback()
            {
                BranchId = model.BranchId,
                TenantId = model.TenantId,
                FeedbackType = model.FeedbackType,
                Language = model.Language,
                Remarks = model.Remarks
            };
            return feedback;
        }
        protected override IEnumerable<string> OnValidation()
        {
            if(_model == null)
            {
                yield return "Invalid or no payload recieved";
            }
            if (_model.TenantId <= 0)
            {
                yield return "Tenant is invalid";
            }
            if (_model.BranchId != null && _model.BranchId <= 0)
            {
                yield return "Branch is invalid";
            }
            if(String.IsNullOrEmpty(_model.Remarks))
            {
                yield return "No feedback given";
            }
        }
    }
}

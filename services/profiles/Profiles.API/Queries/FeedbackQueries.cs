using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public class FeedbackQueries : IFeedbackQueries
    {
        private readonly ProfilesDbContext _ctx;
        
        public FeedbackQueries(ProfilesDbContext ctx)
        {
            _ctx = ctx;
        }
        public Task<IEnumerable<FeedbackModel>> GetFeedbackList(int? branchId, int tenantId, FeedbackType? type)
        {
            List<FeedbackModel> feedback = new List<FeedbackModel>();
            var feedbacks = _ctx.Feedbacks.Where(t => t.TenantId == tenantId).ToList();

            if (type != null)
            {
                feedbacks = feedbacks.Where(p => p.FeedbackType == type).ToList();
            }

            if (branchId != null)
            {
                feedbacks = feedbacks.Where(p => p.BranchId == branchId).ToList();
            }

            foreach(var fb in feedbacks)
            {
                FeedbackModel model = MapToFeedbackModel(fb);
                feedback.Add(model);
            }
            return Task.FromResult(feedback.AsEnumerable());
        }

        private FeedbackModel MapToFeedbackModel(Feedback feedback)
        {
            FeedbackModel model = new FeedbackModel()
            {
                FeedbackId = feedback.Id,
                BranchId = feedback.BranchId,
                TenantId = feedback.TenantId,
                FeedbackType = feedback.FeedbackType,
                Language = feedback.Language,
                Remarks = feedback.Remarks
            };
            return model;
        }
    }
}

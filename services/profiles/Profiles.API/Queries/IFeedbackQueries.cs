using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface IFeedbackQueries
    {
        Task<IEnumerable<FeedbackModel>> GetFeedbackList(int? branchId, int tenantId, FeedbackType? type);
    }
}

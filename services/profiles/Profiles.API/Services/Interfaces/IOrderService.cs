using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface IOrderService
    {
        Task<List<int>> GetCustomerIdsOrderedInBranch(int branchId);
        Task<UpdateExpressBroadcastResponse> UpdateExpressOrderBroadcastDrivers(UpdateExpressBroadcastRequest req);
        Task<bool> UpdateAttachedDistributorForActiveOrders(int customerId, AttachDistributorToOrderRequest req);

        Task<List<RecentCustomerOrder>> GetCustomerRecentOrders();
    }
}

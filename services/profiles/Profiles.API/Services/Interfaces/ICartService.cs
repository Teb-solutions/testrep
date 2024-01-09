using EasyGas.Services.Profiles.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface ICartService
    {
        Task<bool> ConvertTempCart(string tempUserId, string userId);
        Task<bool> UpdateDeliverySlots(int branchId, List<DeliverySlotModel> slots);
    }
}

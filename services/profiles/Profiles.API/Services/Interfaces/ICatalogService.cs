using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Distributor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface ICatalogService
    {
        Task<bool> AddRelaypointItemMasters(int relaypointId, int branchId);
        Task<List<BusinessEntityAssetCount>> GetDealerAssetCountListForDistributor();
        Task<List<AssetCount>> GetDealerAssetCount(int dealerId);
    }
}

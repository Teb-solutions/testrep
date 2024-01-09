using EasyGas.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Profiles.API.ViewModels.Relaypoint;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Profiles.API.ViewModels.Distributor
{
    public class DealerModel
    {
        public BusinessEntityModel Profile { get; set; }
        public List<AssetCount> AssetCounts { get; set; }
        
    }

    public class AssetCount
    {
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public AssetStatus Type { get; set; }
        public int Quantity { get; set; }
    }

    public class BusinessEntityAssetCount
    {
        public int BusinessEntityId { get; set; }
        public List<AssetCount> AssetCounts { get; set; }

    }

    public class DealerDetailModel
    {
        public BusinessEntityModel Profile { get; set; }
        public List<AssetCount> AssetCounts { get; set; }

    }
}


using System.Collections.Generic;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CatalogItemPriceChangedIntegrationEvent : IntegrationEvent
    {
        public List<CatalogItemPriceChangedIntegrationEventModel> ProductList { get; set; }
        public int BranchId { get; set; }
    }

    public class CatalogItemPriceChangedIntegrationEventModel
    { 
        public int ProductVariantId { get; set; }
        //public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }

}

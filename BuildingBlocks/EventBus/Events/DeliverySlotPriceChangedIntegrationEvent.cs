namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class DeliverySlotPriceChangedIntegrationEvent : IntegrationEvent
    {
        public decimal NewPrice { get; set; }
        public int BranchId { get; set; }
        public int DeliverySlotId { get; set; }
        public bool IsSlotExpress { get; set; }
    }
}

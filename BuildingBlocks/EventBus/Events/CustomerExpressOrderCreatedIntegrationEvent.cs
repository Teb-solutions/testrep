
namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CustomerExpressOrderCreatedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public string ItemsSummary { get; set; }
    }
}

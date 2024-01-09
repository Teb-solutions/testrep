
namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class TestIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
    }
}

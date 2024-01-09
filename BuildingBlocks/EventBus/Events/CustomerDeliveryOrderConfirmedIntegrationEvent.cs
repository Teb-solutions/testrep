
namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CustomerDeliveryOrderConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public string UserMobile { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public string DeliverySlotName { get; set; }

        public string OfferCouponName { get; set; }
        public int? OfferCouponId { get; set; }
        public int? OfferCouponType { get; set; }
        public decimal? OfferAmount { get; set; }
    }
}

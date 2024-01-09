
namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CustomerOrderCancelledIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public int? DriverId { get; set; }
        public int? RelaypointId { get; set; }

        public string OfferCouponName { get; set; }
        public int? OfferCouponId { get; set; }
        public int? OfferCouponType { get; set; }
        public decimal? OfferAmount { get; set; }

        public bool IsPickup { get; set; }
        public int? RelaypointOrderBookingId { get; set; }
    }
}

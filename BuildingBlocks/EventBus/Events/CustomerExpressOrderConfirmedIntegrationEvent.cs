
using System.Collections.Generic;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CustomerExpressOrderConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public string UserMobile { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public int DriverId { get; set; }
        public string DeliverySlotName { get; set; }
        public string DeliveryOtp { get; set; }
        public List<ExpressOrderConfirmedEventDriverDetail> BroadcastStopDriverList { get; set; }

        public string OfferCouponName { get; set; }
        public int? OfferCouponId { get; set; }
        public int? OfferCouponType { get; set; }
        public decimal? OfferAmount { get; set; }
    }

    public class ExpressOrderConfirmedEventDriverDetail
    {
        public int DriverId { get; set; }
        public string DriverDeviceId { get; set; }
    }
}

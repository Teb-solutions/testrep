using System;
using System.Collections.Generic;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    /// <summary>
    /// this event is called when driver delivers order to customer or when relaypoint authenticated customer pickup order
    /// </summary>
    public class CustomerOrderDeliveredIntegrationEvent : IntegrationEvent
    {

        public int OrderId { get; set; }

        /// <summary>
        /// if order is a pickup
        /// </summary>
        public bool IsPickupOrder { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public int? DriverId { get; set; }
        public int BusinessEntityId { get; set; }
        public string DeliverySlotName { get; set; }
        public bool DeliveredByAdmin { get; set; }
        public DateTime DeliveredAt { get; set; }

        public decimal TotalProductPrice { get; set; }
        public decimal TotalDeliveryPrice { get; set; }
        public decimal TotalDiscountPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public string OfferCouponName { get; set; }
        public int? OfferCouponId { get; set; }
        public int? OfferCouponType { get; set; }
        public int? ReferredUserId { get; set; }

        public string CustomerLocation { get; set; }
        public string CustomerPincode { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }

        public string InvoiceFilename { get; set; }

        public bool HasRefillItem { get; set; }

        public List<OrderItemEvent> Items { get; set; }
        public List<OrderDigitalVoucherEvent> Vouchers { get; set; }
    }

    public class OrderItemEvent
    {
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderDigitalVoucherEvent
    {
        public string VoucherCode { get; set; }
        public string VoucherFilename { get; set; }
        public decimal Deposit { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class OrderDetailsReportVM
    {
        public List<ReportOrderModel> OrdersList { get; set; }
        public List<ReportUnapprovedOrderModel> UnapprovedOrdersList { get; set; }
    }

    public class ReportOrderModel
    {
        public string Code { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public string DeliverySlotName { get; set; }
        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliveryTo { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }

        public string AddressLocation { get; set; }
        public double AddressLat { get; set; }
        public double AddressLng { get; set; }
        public int AddressPinCode { get; set; }

        //public OrderStatus Status { get; set; }
        public string Status { get; set; }
        public string VehicleRegNo { get; set; }
        public DateTime? VehicleAssignedAt { get; set; }
        public DateTime? PlannedDeliveryFrom { get; set; }
        public DateTime? PlannedDeliveryTo { get; set; }

        public bool? AssignedManually { get; set; }

        public DateTime? DispatchedAt { get; set; }
        public int? DeliveredByDriverName { get; set; }
        public int? DeliveredByVehicleIRegNo { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveryRemarks { get; set; }

        public DateTime? CancelledAt { get; set; }
        public string CancelledRemarks { get; set; }

        public float TotalAmount { get; set; }
        public float OfferAmount { get; set; }
        public float PayableAmount { get; set; }
        public string OfferCouponName { get; set; }

        public bool? PaymentCompleted { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }
        public bool? IsRefunded { get; set; }
        public DateTime? RefundedAt { get; set; }

        public string Source { get; set; }

        public DateTime CreatedAt { get; set; }
        public float? CustomerRating { get; set; }


        public float? DriverRating { get; set; }

        public String CustomerFeedback { get; set; }

        public String DriverFeedback { get; set; }

        
    }

    public class ReportUnapprovedOrderModel
    {
        public string Code { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public string DeliverySlotName { get; set; }
        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliveryTo { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }

        public string AddressLocation { get; set; }
        public double AddressLat { get; set; }
        public double AddressLng { get; set; }
        public int AddressPinCode { get; set; }

        //public OrderStatus Status { get; set; }
        public string Status { get; set; }

        public float TotalAmount { get; set; }
        public float OfferAmount { get; set; }
        public float PayableAmount { get; set; }
        public string OfferCouponName { get; set; }

        public string Source { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}

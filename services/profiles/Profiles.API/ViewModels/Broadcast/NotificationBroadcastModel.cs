using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Broadcast
{
    public class NotificationBroadcastModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        //public virtual User User { get; set; }
        //public UserType UserType { get; set; }
        public string DeviceId { get; set; }
        public string Description { get; set; }
        public string BookingCode { get; set; }
        public int BookingId { get; set; }
        public string BookingType { get; set; }
        public string CustAddress { get; set; }
        public string CustName { get; set; }
        public string CustMobile { get; set; }
        public string TimeOfDelivery { get; set; }
        public int TimeOutSec { get; set; }
        public string CylinderQuantity { get; set; }
        //public NotificationType Type { get; set; }
        public BroadcastType BroadcastType { get; set; }
        //public NotificationStatus Status { get; set; }
    }

    public enum BroadcastType
    {
        BROADCAST = 1,
        ASSIGN,
        STOP,
        REJECT
    }
}

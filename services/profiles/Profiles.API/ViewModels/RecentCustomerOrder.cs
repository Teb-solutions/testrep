using System;

namespace Profiles.API.ViewModels
{
    public class RecentCustomerOrder
    {
        public int UserId { get; set; }
        public DateTime LastOrderedAt { get; set; }
        public DateTime? LastOrderDeliveredAt { get; set; }
    }
}

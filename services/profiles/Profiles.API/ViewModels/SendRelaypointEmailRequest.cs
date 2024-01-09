using System;
using System.Collections.Generic;

namespace Profiles.API.ViewModels
{
    public class SendRelaypointEmailRequest
    {
        public string AttachmentUrl { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class SendStockRequestEmailToAdmin
    {
        public List<OrderRequest> OrderRequests { get; set; }
    }

    public class OrderRequest
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
    }
}

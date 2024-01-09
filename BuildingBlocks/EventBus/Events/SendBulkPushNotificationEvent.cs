using System;
using System.Collections.Generic;
using System.Text;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class SendBulkPushNotificationEvent : IntegrationEvent
    {
        public bool IsUserCustomer { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsImageActive { get; set; }
        public string Imageurl { get; set; }
        public List<string> UserMobileList { get; set; }
    }
}

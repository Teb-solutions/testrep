using System;
using System.Collections.Generic;
using System.Text;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class CustomerOrderDispatchedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string OrderDeliveryOTP { get; set; }
        public int UserId { get; set; }
    }
}


using System.Collections.Generic;

namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class RoutePlanChangedIntegrationEvent : IntegrationEvent
    {
        public int BranchId { get; set; }
        public int TenantId { get; set; }
        public List<int> OrderAssignedDriverIds { get; set; }
    }
}

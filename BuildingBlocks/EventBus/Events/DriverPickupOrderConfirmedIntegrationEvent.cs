
namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class DriverPickupOrderConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Code { get; set; }
        public int RelaypointId { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public string OTP { get; set; }
    }
}


namespace EasyGas.BuildingBlocks.EventBus.Events
{
    public class AverageRatingChangedIntegrationEvent : IntegrationEvent
    {
        public int UserId { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsDriver { get; set; }
        public bool IsRelaypoint { get; set; }
        public float AverageRating { get; set; }
    }
}

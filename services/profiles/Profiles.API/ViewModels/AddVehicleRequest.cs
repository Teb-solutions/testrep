
namespace EasyGas.Services.Profiles.Models
{
    public class AddVehicleRequest
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int BranchId { get; set; }
        public int? DriverId { get; set; }
        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }
        public string RegNo { get; set; }
        public bool IsActive { get; set; }
        public int MaxCylinders { get; set; }
        public double LastLat { get; set; }
        public double LastLng { get; set; }
        public double OriginLat { get; set; }
        public double originLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public VehicleState State { get; set; }
    }
}

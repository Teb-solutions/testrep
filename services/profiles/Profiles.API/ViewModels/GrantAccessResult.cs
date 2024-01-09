using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class GrantAccessResult
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int TenantId { get; set; }
        public int? CityId { get; set; }
        public UserType UserType { get; set; }
    }

    public class CustomerGrantAccessResult : GrantAccessResult
    {
    }

    public class DriverGrantAccessResult : GrantAccessResult
    {
        public int? VehicleId { get; set; }
        public string VehicleRegNo { get; set; }
    }

    public class AdminGrantAccessResult : GrantAccessResult
    {
        public string CityName { get; set; }
        public float? CityLat { get; set; }
        public float? CityLng { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> Roles { get; set; }
    }

    public class BusinessEntityGrantAccessResult : GrantAccessResult
    {
        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }
    }

    public class RelaypointGrantAccessResult : GrantAccessResult
    {
        public int? RelaypointId { get; set; }
        public string RelaypointName { get; set; }
    }

    public class DistributorGrantAccessResult : GrantAccessResult
    {
        public int? DistributorId { get; set; }
        public string DistributorName { get; set; }
    }
}

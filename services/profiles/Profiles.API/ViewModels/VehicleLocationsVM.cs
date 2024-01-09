using EasyGas.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class VehicleLocationsViewModel
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string UserId { get; set; }
        public Source Source { get; set; }
        public int TenantId { get; set; }
        public int VehicleId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedAtTimestamp { get; set; }
    }

}

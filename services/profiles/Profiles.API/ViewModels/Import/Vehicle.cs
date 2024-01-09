using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Import
{
    public class Vehicle : Trackable
    {
        public int Id { get; set; }
        public int? DriverId { get; set; }
        public virtual User Driver { get; set; }
        public int? DistributorId { get; set; }
        public virtual User Distributor { get; set; }
        public string RegNo { get; set; }
        public int TypeId { get; set; }
        public bool IsActive { get; set; }
        public int MaxCylinders { get; set; }
        public double OriginLat { get; set; }
        public double originLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public VehicleState State { get; set; }
    }

    public enum VehicleState
    {
        ReadyForWork,
        OutFromWork,
        Break
    }
}

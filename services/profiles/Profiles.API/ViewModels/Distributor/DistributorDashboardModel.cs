
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared;
using System;
using System.Collections.Generic;

namespace Profiles.API.ViewModels.Distributor
{
    public class DistributorDashboardModel
    {
        public int TotalVehicles { get; set; }
        public int VehiclesActive { get; set; }
        public int VehiclesInBreak { get; set; }
        public int VehiclesLogoff { get; set; }

        public int TotalCustomers { get; set; }
        public int NewCustomersToday { get; set; }

        public List<DistributorVehicleModel> Vehicles { get; set; }
        public List<DistributorCustomerModel> RecentCustomers { get; set; }
    }

    public class DistributorCustomerModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public bool IsReferredByDistributor { get; set; }
        public Source? Source { get; set; }
    }

    public class DistributorVehicleModel
    {
        public int Id { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverMobile { get; set; }

        public string RegNo { get; set; }
        public bool IsActive { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public double? LastLocationLat { get; set; }
        public double? LastLocationLng { get; set; }
        public DateTime? LastLocationUpdatedAt { get; set; }

        public VehicleState State { get; set; }

        public static DistributorVehicleModel FromVehicleModel(VehicleModel veh)
        {
            DistributorVehicleModel model = new DistributorVehicleModel()
            {
                DestinationLat = veh.DestinationLat,
                DestinationLng = veh.DestinationLng,
                DriverId = veh.DriverId,
                DriverMobile = veh.DriverMobile,
                DriverName = veh.DriverName,
                IsActive = veh.IsActive,
                OriginLat = veh.OriginLat,
                OriginLng = veh.OriginLng,
                RegNo = veh.RegNo,
                State = veh.State,
                Id = veh.Id,
            };

            return model;
        }
    }
}

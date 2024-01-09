﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int BranchId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverMobile { get; set; }
        public string DriverDeviceId { get; set; }

        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }
        public string BusinessEntityMobile { get; set; }
        public string BusinessEntityUpiPaymentNumber { get; set; }
        public string BusinessEntityUpiQrCodeUrl { get; set; }
        public double? BusinessEntityLat { get; set; }
        public double? BusinessEntityLng { get; set; }

        public string RegNo { get; set; }
        public bool IsActive { get; set; }
        public int MaxCylinders { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public VehicleState State { get; set; }
        public DLoginState DriverLoginState { get; set; }
        public DActivityState DriverActivityState { get; set; }
        public DateTime? DriverLoginStateTime { get; set; }
        public DateTime? DriverActivityStateTime { get; set; }
        public int DriverLoginStateTimeAgo { get; set; }
        public int DriverActivityStateTimeAgo { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime? LocUpdatedAt { get; set; }
        public int LocUpdatedTimeAgoMin { get; set; }
    }
}

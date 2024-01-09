using EasyGas.Shared.Enums;
using System;

namespace Profiles.API.ViewModels.Relaypoint
{
    public class BusinessEntityModel
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }

        public int? ParentBusinessEntityId { get; set; }
        public string ParentBusinessEntityName { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public BusinessEntityType Type { get; set; }

        //address
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        //public Geometry GeoLocation { get; set; }
        public string Details { get; set; }
        public string Landmark { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }

        public DayOfWeek? WorkingStartDay { get; set; }
        public DayOfWeek? WorkingEndDay { get; set; }
        public string WorkingStartTime { get; set; }
        public string WorkingEndTime { get; set; }

        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string PaymentNumber { get; set; }
        public string UPIQRCodeImageUrl { get; set; }

        public float Rating { get; set; }

        public bool IsActive { get; set; }

        public string CoverImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }

        public double DistanceFromOrigin { get; set; }
    }
}

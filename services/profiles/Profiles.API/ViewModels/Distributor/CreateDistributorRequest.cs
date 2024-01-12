﻿using EasyGas.Shared;

namespace Profiles.API.ViewModels.Distributor
{
    public class CreateDealerRequest
    {
        public int? Id { get; set; }
        public int ParentBusinessEntityId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }

        //address
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        //public SqlGeography LatLongLocation { get; set; }
        public string Details { get; set; }
        public string Landmark { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }

        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string PaymentNumber { get; set; }
        public string UPIQRCodeImageUrl { get; set; }

        public bool IsActive { get; set; }

        public string AdminUserName { get; set; }
        public string AdminPassword { get; set; }
        public string AdminFirstName { get; set; }
        public string AdminLastName { get; set; }
        public string AdminMobile { get; set; }
        public string AdminEmail { get; set; }

        public Source Source { get; set; }

        //public List<WorkingDaysModel> WorkingDaysList { get; set; }
    }
}
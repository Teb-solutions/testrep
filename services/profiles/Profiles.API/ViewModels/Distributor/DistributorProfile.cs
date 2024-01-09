using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Profiles.API.ViewModels.Distributor
{
    public class UpdateDistributorProfile
    {
        [Required]
        [MinLength(3)]
        public string Name { get; set; }
        [Required]
        [MinLength(9)]
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string UpiPaymentNumber { get; set; }
    }

    public class DistributorProfile
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public float Rating { get; set; }
        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string UpiPaymentNumber { get; set; }
    }

    public class UpdateDistributorAddress
    {
        [Required]
        public string Location { get; set; }
        [Required]
        public double Lat { get; set; }
        [Required]
        public double Lng { get; set; }
        [Required]
        public string Details { get; set; }
        public string Landmark { get; set; }
        //public string State { get; set; }
        public string PinCode { get; set; }
    }

    public class AddDistributorVehicleRequest
    {
        //public int Id { get; set; }
        public int? DriverId { get; set; }
        public string RegNo { get; set; }
        public bool IsActive { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
    }

    public class DistributorCreateVehicleVM
    {
        public List<SelectListItem> DriverSelectList { get; set; }
    }

    public class DistributorUpdateVehicleVM
    {
        public List<SelectListItem> DriverSelectList { get; set; }
        public AddDistributorVehicleRequest Vehicle { get; set; }
    }

    public class AddDistributorDriverRequest
    {
        //public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }

    public class DistributorDriverModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}

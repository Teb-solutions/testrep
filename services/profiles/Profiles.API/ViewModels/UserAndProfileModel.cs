using EasyGas.Shared;
using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class UserAndProfileModel
    {
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
        public DateTime? CreatedAt { get; set; }
        public CreationType CreationType { get; set; }
        public string UpdatedBy { get; set; }

        public Otp OtpModel { get; set; }
        public DayOfWeek? OffDay { get; set; }
        public Source? Source { get; set; }

        public String DeviceId { get; set; }
        public float? AverageRating { get; set; }
        public string PhotoUrl { get; set; }
        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string Code { get; set; }
        public string ReferralCode { get; set; }
        public string MyReferralCode { get; set; }
        public string ReferredByUserFullName { get; set; }

        public bool OtpValidated { get; set; }
        public DateTime? OtpValidatedAt { get; set; }
        public string OtpValidatedBy { get; set; }

        public bool AgreedTerms { get; set; }
        public bool IsAmbassador { get; set; }

        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }
        public string BusinessEntityBranchName { get; set; }

        public double? RegisteredFromLat { get; set; }
        public double? RegisteredFromLng { get; set; }
    }

    public class CustomerProfileModel
    {
        public int? CityId { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        
        //public CreationType CreationType { get; set; }
        //public string UpdatedBy { get; set; }

        public float? AverageRating { get; set; }
        public string PhotoUrl { get; set; }
        public bool SendNotifications { get; set; }

        public string MyReferralCode { get; set; }
        public string ReferredByUserFullName { get; set; }
        public string ReferralCode { get; set; }

        public bool OtpValidated { get; set; }

        public DateTime? CreatedAt { get; set; }
        //public bool AgreedTerms { get; set; }
        //public bool IsAmbassador { get; set; }
        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }
        public string BusinessEntityBranchName { get; set; }
    }

    public class UpdateCustomerProfileModel
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool SendNotifications { get; set; }
    }

    public class DriverProfileModel
    {
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public int? BusinessEntityId { get; set; }
        public string BusinessEntityName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
        public float? AverageRating { get; set; }
        public string PhotoUrl { get; set; }
        public AddressModel Address { get; set; }

        public int? VehicleId { get; set; }
        public string VehicleName { get; set; }
        public double VehicleOriginLat { get; set; }
        public double VehicleOriginLng { get; set; }
        public double VehicleDestinationLat { get; set; }
        public double VehicleDestinationLng { get; set; }
        public DayOfWeek? OffDay { get; set; }
     
    }

    public class UpdateDriverProfileModel
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class StaffModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }

        public static StaffModel FromUser(User user)
        {
            return new StaffModel
            {
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Mobile = user.Profile.Mobile,
                Email = user.Profile.Email,
                UserType = user.Type,
                CreatedAt = user.CreatedAt
            };
        }

        public static StaffModel FromUserAndProfile(UserAndProfileModel user)
        {
            return new StaffModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mobile = user.Mobile,
                Email = user.Email,
                UserType = user.Type,
                CreatedAt = (DateTime)user.CreatedAt
            };
        }
    }

    public class BackendUserProfileModel
    {
        public int TenantId { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }
        public string PhotoUrl { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public UserType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; }

        public static BackendUserProfileModel FromUser(User user)
        {
            return new BackendUserProfileModel()
            {
                TenantId = user.TenantId,
                CityId = user.BranchId,
                CityName = user.Branch?.Name,
                CreatedAt = user.CreatedAt,
                Email = user.Profile.Email,
                Name = user.Profile.GetFullName(),
                Mobile = user.Profile.Mobile,
                PhotoUrl = user.Profile.PhotoUrl,
                Type = user.Type,
                UserId = user.Id,
                UserName = user.UserName,
                Roles = user.Roles?.Select(p => p.Role.DisplayName).ToList()
            };
        }
    }
}

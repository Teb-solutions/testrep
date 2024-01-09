using EasyGas.Services.Profiles.Models;
using EasyGas.Shared;
using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;

namespace Profiles.API.ViewModels.Import
{
    public class User : Trackable
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public DateTime? LastLogin { get; set; }

        public string Password { get; set; }
        public UserType Type { get; set; }
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        public int? DistributorId { get; set; } // userId of distributor
        public virtual User Distributor { get; set; }
        public CreationType CreationType { get; set; }
        public bool OtpValidated { get; set; }
        public DateTime? OtpValidatedAt { get; set; }
        public string OtpValidatedBy { get; set; }

        public virtual UserProfile Profile { get; set; }

        public ICollection<UserAddress> Addresses { get; set; }
    }

    public class UserProfile : Trackable
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public Guid? FaceProfileId { get; set; }

        public Guid? VoiceProfileId { get; set; }

        public string VoiceSecretPhrase { get; set; }

        public string Skype { get; set; }
        public string Mobile { get; set; }

        public string PhotoUrl { get; set; }
        public DayOfWeek? OffDay { get; set; }
        public Source? Source { get; set; }

        public string ReferralCode { get; set; } // myreferralcode of other user who has referred this user
        public int? ReferredByUserId { get; set; }
        public User ReferredByUser { get; set; }
        public string DeviceId { get; set; }
        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string Code { get; set; }
        public string MyReferralCode { get; set; } // code generated to be given as referral code to other users to get points
        public string VendorPaymentAccountId { get; set; } // razorpay acc id linked to user
        public bool AgreedTerms { get; set; }
        public string PaymentNumber { get; set; }
        public string UPIQRCodePath { get; set; }

    }

    public class UserAddress : Trackable
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string BuildingNo { get; set; }
        public string StreetNo { get; set; }
        public string Landmark { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public int PinCode { get; set; }
        public string PhoneAlternate { get; set; }
    }

    public enum Gender
    {
        NotSpecified,
        Male,
        Female
    }
}

using EasyGas.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
  
    public class UserProfile : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string PhotoUrl { get; set; }
        public DayOfWeek? OffDay { get; set; }
        public Source? Source { get; set; }
        
        public string ReferralCode { get; set; } // myreferralcode of other user who has referred this user
        public int? ReferredByUserId { get; set; }
        [ForeignKey("ReferredByUserId")]
        public User ReferredByUser { get; set; }
        public string DeviceId { get; set; }
        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string Code { get; set; }
        public string MyReferralCode { get; set; } // code generated to be given as referral code to other users to get points
        public string VendorPaymentAccountId { get; set; } // razorpay acc id linked to user
        public bool AgreedTerms { get; set; }
        public bool SendNotifications { get; set; }
        public float Rating { get; set; }

        public DateTime? LastOrderedAt { get; set; }
        public DateTime? LastOrderDeliveredAt { get; set; }

        public double? RegisteredFromLat { get; set; }
        public double? RegisteredFromLng { get; set; }

        public UserProfile()
        {
            Gender = Gender.NotSpecified;
        }

        public string GetFullName()
        {
            string fullName = FirstName;
            if (!string.IsNullOrEmpty(LastName))
            {
                fullName +=" " + LastName;
            }
            return fullName;
        }
    }
}

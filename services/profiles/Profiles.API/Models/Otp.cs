using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Otp : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Value { get; set; }
        public string SendTo { get; set; }
        public OtpReceiver Receiver { get; set; }
        public OtpType Type { get; set; }
        public bool HasExpiry { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsValidated { get; set; }
        public int? UserId { get; set; }
        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
    }

    public enum OtpReceiver
    {
        CUSTOMER,
        DRIVER,
        ADMIN
    }

    public enum OtpType
    {
        CUSTOMER_REGISTER,
        CUSTOMER_RESET_PASSWORD,
        CUSTOMER_CREATE_ORDER,
        CUSTOMER_LOGIN,
        CUSTOMER_CHANGE_MOBILE
    }
}

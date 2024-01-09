using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations ;
using System.ComponentModel.DataAnnotations.Schema;
using Profiles.API.Models;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Models
{
    public class User : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public DateTime? LastLogin { get; set; }

        public string Password { get; set; }
        public UserType Type { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int? BusinessEntityId { get; set; } // Id of distributor/relaypoint
        [ForeignKey("BusinessEntityId")]
        public virtual BusinessEntity BusinessEntity { get; set; }
        public DateTime? BusinessEntityAttachedAt { get; set; }
        public int? BusinessEntityAttachedByUserId { get; set; }
        [ForeignKey("BusinessEntityAttachedByUserId")]
        public virtual User BusinessEntityAttachedBy { get; set; }

        public CreationType CreationType { get; set; }
        public bool OtpValidated { get; set; }
        public DateTime? OtpValidatedAt { get; set; }
        public string OtpValidatedBy { get; set; }

        public virtual UserProfile Profile { get; set; }
        public ICollection<UserAddress> Addresses { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<UserRole> Roles { get; set; } = new List<UserRole>();
        public ICollection<UserLogin> Logins { get; set; }
        //public ICollection<UserItem> Items { get; set; }
        //public ICollection<DigitalSV> DigitalSvs { get; set; }
    }

    public enum Gender
    {
        NotSpecified,
        Male,
        Female
    }
}

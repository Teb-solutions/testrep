using EasyGas.Services.Profiles.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Profiles.API.Models
{
    public class Complaint : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId ")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId ")]
        public virtual Branch Branch { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId ")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }
        [Required]
        [StringLength(1000)]
        public string Message { get; set; }
        [StringLength(200)]
        public string AttachmentUrl { get; set; }
        [Required]
        public ComplaintCategory Category { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; }

        public ComplaintStatus Status { get; set;}
        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        [ForeignKey("ClosedByUserId ")]
        public virtual User ClosedByUser { get; set; }

        public DateTime? ReOpenAt { get; set; }
        public int? ReOpenByUserId { get; set; }
        [ForeignKey("ReOpenByUserId ")]
        public virtual User ReOpenByUser { get; set; }
        [StringLength(1000)]
        public string ReOpenReason { get; set; }
    }

    public enum ComplaintCategory
    {
        Information = 1,
        Payment,
        Complaint,
        Others
    }

    public enum ComplaintStatus
    {
        [Display(Description = "Closed")]
        Closed = 0,
        [Display(Description = "Open")]
        Open,
        [Display(Description = "Reopened")]
        ReOpened,
    }
}

using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class NotificationSettings : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public string Name { get; set; }
        public NotificationCategory NotiCategory { get; set; }
        public NotificationType NotiType { get; set; }
        public NotificationTriggerType NotiTriggerType { get; set; }
        public NotificationUserCategory NotiUserCategory { get; set; }
        public NotificationTriggerTime NotiTriggerTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public int? Frequency { get; set; }
        public DateTime? ScheduledDate { get; set; }
        [MaxLength(1000)]
        public string Title { get; set; }
        [MaxLength(3000)]
        public string Description { get; set; }
        public bool IsImageActive { get; set; }
        public string Imageurl { get; set; }
        public string GoToWebUrl { get; set; }
        public bool IsWebview { get; set; }
        public string WebviewUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastCronJobRunAt { get; set; } // time at which last cron job was run
        [NotMapped]
        public string ImageBase64String { get; set; }
        [NotMapped]
        public string ImageExtension { get; set; }
        [NotMapped]
        public string UserMobile { get; set; }
    }

    public class NotificationTemplates : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public NotificationType NotiType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}

using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Profiles.API.ViewModels.Notification
{
    public class CustomNotificationRequest
    {
        public int? TenantId { get; set; }
        public int? BranchId { get; set; }
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
        public string ImageBase64String { get; set; }
        public string ImageExtension { get; set; }
        public List<string> UserMobileList { get; set; }
    }
}

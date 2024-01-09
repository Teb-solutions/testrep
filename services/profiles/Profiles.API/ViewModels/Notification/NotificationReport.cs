using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using System;

namespace Profiles.API.ViewModels.Notification
{
    public class NotificationReport
    {
        public string UserMobile { get; set; }
        public string UserFullName { get; set; }
        public int? UserId { get; set; }
        public UserType UserType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string GoToWebUrl { get; set; }
        public string DeviceId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationCategory Category { get; set; }
        public PushNotificationType? PushNotificationType { get; set; }
        public NotificationStatus Status { get; set; }
        public NotificationTriggerType? TriggerType { get; set; }
        public NotificationUserCategory? UserCategory { get; set; }
        public NotificationTriggerTime? TriggerTime { get; set; }
        public DateTime SentAt { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}

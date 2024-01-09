using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Notification : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
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
        public string FirebaseResponse { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        [NotMapped]
        public string OrderCode { get; set; }
    }

    public enum NotificationType
    {
        INFO = 1,
        ALERT,
        WARNING,
    }

    public enum NotificationCategory
    {
        DRIVER_BROADCAST_START = 1,
        DRIVER_BROADCAST_STOP = 2,
        DRIVER_ORDER_CHANGED = 3,
        DRIVER_ORDER_ASSIGNED = 4,
        CUSTOMER_ORDER_STATUS_CHANGE = 5,
        CUSTOMER_PROMOTIONS = 6,
        CUSTOMER_ORDER_CONFIRMED,
        CUSTOMER_ORDER_DISPATCHED,
        CUSTOMER_ORDER_DELIVERED,
        CUSTOMER_ORDER_CUSTOMER_CANCELLED,
        CUSTOMER_ORDER_ORDER_ADMIN_CANCELLED,
        CUSTOMER_ORDER_PAYMENT_COMPLETE,
        CUSTOMER_ORDER_PAYMENT_REFUND,
        CUSTOMER_PICKUP_ORDERED,

        /// <summary>
        /// customer surrender requested
        /// </summary>
        CUSTOMER_DROP_ORDERED,

        /// <summary>
        /// driver reserved order
        /// </summary>
        DRIVER_PICKUP_ORDERED,

        /// <summary>
        /// driver delivered refill order or customer cancelled order
        /// </summary>
        DRIVER_DROP_ORDERED,

    }

    public enum NotificationStatus
    {
        UNREAD,
        READ
    }

    public enum PushNotificationType
    {
        DATA,
        NOTIFICATION
    }

    public enum NotificationTriggerType
    {
        AUTO_SEND_FREQUENCY,
        AUTO_SEND_SCHEDULED,
        MANUAL_SEND,
        APP_LOGIN_POPUP,
    }

    public enum NotificationUserCategory
    {
        CUSTOMER_INSTALLED_NOT_REGISTERED = 0,
        CUSTOMER_REGISTERED = 1,
        CUSTOMER_REGISTERED_NOT_ORDERED = 2,
        CUSTOMER_ORDERED = 3,
        CUSTOMER_INSTALLED = 4, // includes registered also
    }

    public enum NotificationTriggerTime
    {
        ON_TRIGGER = 0,
        RANDOM = 1,
        FIXED = 2,
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Serilog.Sinks.MicrosoftTeams.Alternative.Extensions;
using Microsoft.Extensions.Azure;

namespace EasyGas.Services.Profiles.Models
{

    public class NotificationTemplate : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string NotificationName { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? CouponCode { get; private set; }
        public bool IsActive { get; private set; }
        public NotificationChannel Channel { get; private set; }

        public NotificationTemplate(string notificationName, string title, string description, string imageUrl, string couponCode, bool isActive, NotificationChannel channel)
        {
            NotificationName = notificationName;
            Title = title;
            Description = description;
            ImageUrl = imageUrl;
            CouponCode = couponCode;
            IsActive = isActive;
            Channel = channel;
        }

        public NotificationTemplate Update(string notificationName, string title, string description, string imageUrl, string couponCode, bool isActive, NotificationChannel channel)
        {
            if (!string.IsNullOrEmpty(notificationName) && !NotificationName.Equals(notificationName)) NotificationName = notificationName;
            if(!string.IsNullOrEmpty(title) && !Title.Equals(title)) Title = title;
            if(!string.IsNullOrEmpty(description) && !Description.Equals(description)) Description = description;
            if (!string.IsNullOrEmpty(imageUrl) && !ImageUrl.Equals(imageUrl)) ImageUrl = imageUrl;
            if (!string.IsNullOrEmpty(couponCode) && !CouponCode.Equals(couponCode)) CouponCode = couponCode;
            if(!IsActive.Equals(isActive))IsActive = isActive;
            if (!Channel.Equals(channel)) Channel = channel;
            return this;
        }
    }

    

    public enum NotificationChannel
    {
        App = 1,
        Whatsapp
    }
}

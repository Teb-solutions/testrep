using EasyGas.Services.Profiles.Models;

namespace Profiles.API.ViewModels.Notification
{
    public class AddNotificationTemplateRequest
    {
        public string NotificationName { get; set; }
        public string Title { get; set; }
        public string Description { get;  set; }
        public string? CouponCode { get; set; }
        public bool IsActive { get; set; }
        public string ImageBase64String { get; set; }
        public string ImageExtension { get; set; }
        public NotificationChannel Channel { get; set; }
    }

    public class UpdateNotificationTemplateRequest
    {
        public int Id { get; set; }
        public string NotificationName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? CouponCode { get; set; }
        public bool IsActive { get; set; }
        public string? ImageBase64String { get; set; }
        public string? ImageExtension { get; set; }
        public NotificationChannel Channel { get; set; }
    }
}

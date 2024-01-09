using Profiles.API.Models;

namespace Profiles.API.ViewModels.AppImage
{
    public class AddAppImageRequest
    {
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public string ImageBase64 { get; set; }
        public AppImageType Type { get; set; }
        public int Position { get; set; }
    }
}

using EasyGas.Services.Profiles.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Profiles.API.Models
{
    public class AppImage : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public AppImageType Type { get; set; }
        public string FileName { get; set; }
        public int Position { get; set; }
    }
    public enum AppImageType
    {
        CustomerAppHomepage = 1,
        CustomerAppSafety,
    }
}

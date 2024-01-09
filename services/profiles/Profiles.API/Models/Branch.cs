using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Branch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
        public string Mobile { get; set; }
        public bool IsActive { get; set; }
        public string CallCenterNumber { get; set; }
        public string PictureUrl { get; set; }
        public string Email { get; set; }
    }
}

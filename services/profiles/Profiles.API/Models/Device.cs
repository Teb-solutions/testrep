using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Device : Trackable
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

        public int? BusinessEntityId { get; set; }
        [ForeignKey("BusinessEntityId")]
        public virtual BusinessEntity BusinessEntity { get; set; }

        public int? ParentDeviceId { get; set; }
        [ForeignKey("ParentDeviceId")]
        public virtual Device ParentDevice { get; set; }

        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public string? UniqueId { get; set; }
        public string? SecretKey { get; set; }
        public FuelType? FuelType { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Device> ChildDevices { get; set; }
    }

    public enum DeviceType
    {
        Dispenser = 1,
        Nozzle
    }

    public enum FuelType
    {
        LPG = 1,
    }
}

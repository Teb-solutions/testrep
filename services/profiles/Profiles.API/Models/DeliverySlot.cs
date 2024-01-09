using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class DeliverySlot : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId ")]
        public virtual Tenant Tenant { get; set; }
        public int BranchId { get; set; }
        [ForeignKey("BranchId ")]
        public virtual Branch Branch { get; set; }

        public string UID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public SlotType Type { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public DateTime? From { get; set; }
        public int FromSec {get; set; }
        public DateTime? To { get; set; }
        public int ToSec { get; set; }
        public int MaxThreshold { get; set; }
        [Required]
        public decimal Price { get; set; }
        public bool IsActive { get; set; }

    }




}

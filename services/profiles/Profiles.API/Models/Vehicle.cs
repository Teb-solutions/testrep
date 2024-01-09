using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Vehicle : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual User Driver { get; set; }
        public int? BusinessEntityId { get; set; } // Id of distributor/relaypoint
        [ForeignKey("BusinessEntityId")]
        public virtual BusinessEntity BusinessEntity { get; set; }
        public string RegNo { get; set; }
        public bool IsActive { get; set; }
        public int MaxCylinders { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public VehicleState State { get; set; } 
    }

    public enum VehicleState
    {
        ReadyForWork,
        OutFromWork,
        Break
    }

}

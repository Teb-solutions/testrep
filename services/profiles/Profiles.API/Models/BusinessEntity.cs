
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace Profiles.API.Models
{
    public class BusinessEntity : Trackable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int? ParentBusinessEntityId { get; set; }
        [ForeignKey("ParentBusinessEntityId")]
        public virtual BusinessEntity ParentBusinessEntity { get; set; }

        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public BusinessEntityType Type { get; set; }

        //address
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Geometry GeoLocation { get; set; }
         public string Details { get; set; }
        public string Landmark { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }

        public DayOfWeek? WorkingStartDay { get; set; }
        public DayOfWeek? WorkingEndDay { get; set; }
        public string WorkingStartTime { get; set; }

        public string WorkingEndTime { get; set; }

        public float Rating { get; set; }

        public string GSTN { get; set; }
        public string PAN { get; set; }
        public string PaymentNumber { get; set; }
        public string UPIQRCodeImageUrl { get; set; }

        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public  bool IsActive { get; set; }

        public ICollection<BusinessEntityTiming> Timings { get; set; }
    }
}

using EasyGas.Services.Profiles.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Profiles.API.Models
{
    public class BusinessEntityTiming : Trackable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public int BusinessEntityId { get; set; }
        [ForeignKey("BusinessEntityId")]
        public virtual BusinessEntity BusinessEntity { get; set; }

        public DayOfWeek Day { get; set; }
        public  bool IsActive { get; set; }

    }
}

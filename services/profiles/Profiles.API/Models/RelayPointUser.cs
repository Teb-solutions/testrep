using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RelayPointLogistics.Models
{
    public class RelayPointUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int Id { get; set; }

        //public int RelayPointId { get; set; }
        //[ForeignKey("RelayPointId")]
        //public virtual RelayPoint Rpoint { get; set; } 
        
        public int UserId { get; set; }




    }
}

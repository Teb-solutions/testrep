using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Relaypoint
{
    public class UpdateRelaypointProfile
    {
        [Required]
        [MinLength(3)]
        public string Name { get; set; }
        [Required]
        [MinLength(9)]
        public string MobileNumber { get; set; }
        public string Email { get; set; }
    }

    public class RelaypointProfile
    {
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public float Rating {get; set;}
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Profiles.API.ViewModels.Relaypoint
{
    public class UpdateRelaypointAddress
    {
        [Required]
        public string Location { get; set; }
        [Required]
        public double Lat { get; set; }
        [Required]
        public double Lng { get; set; }
        [Required]
        public string Details { get; set; }
        public string Landmark { get; set; }
        //public string State { get; set; }
        public string PinCode { get; set; }
    }
}

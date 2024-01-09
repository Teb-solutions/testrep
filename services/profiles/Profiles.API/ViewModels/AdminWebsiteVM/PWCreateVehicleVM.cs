using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class PWCreateVehicleVM
    {
        public List<SelectListItem> DriverSelectList { get; set; }
        public List<SelectListItem> DistributorSelectList { get; set; }
        public List<SelectListItem> RelaypointSelectList { get; set; }
        public VehicleModel Vehicle { get; set; }
    }
}

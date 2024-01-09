using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class PvtWebDashboardVM
    {
        public int CrmTicketsOpenToday { get; set; }
        public int CrmTicketsOpen { get; set; }
        public int CrmTicketsClosedToday { get; set; }
        public int CrmTicketsClosed { get; set; }

        public List<VehicleModel> Vehicles { get; set; }
    }
}

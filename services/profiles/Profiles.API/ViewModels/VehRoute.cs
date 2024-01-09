using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class VehRoute
    {
        public VehicleModel vehCurrent { get; set; }
        //public List<OrderModel> orderList { get; set; }
        public int totalRoutes { get; set; }
        public double distCovered { get; set; }
        public double filledCapacity { get; set; }
        public double totalArcCost { get; set; }
        public double totalFirstSolCost { get; set; }
        public double vehFixedCost { get; set; }
        public double totalTravelCost { get; set; }
    }
}

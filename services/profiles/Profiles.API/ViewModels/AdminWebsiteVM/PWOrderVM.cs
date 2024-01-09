using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class PWOrderVM
    {
        //public List<OrderModel> OrdersList { get; set; }
        //public List<OrderModel> UnApprovedOrdersList { get; set; }
        public List<VehicleModel> VehiclesList { get; set; }
        public Settings Settings { get; set; }
        public List<VehRoute> VehRouteList { get; set; }
        public List<DeliverySlotModel> DeliverySlotList { get; set; }
    }
}

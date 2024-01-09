using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Broadcast
{
    public class UpdateExpressBroadcastResponse
    {
        public List<VehicleModel> Vehicles { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string OrderAddress { get; set; }
        public int TimeOutSec { get; set; }
        public DateTime DeliveryTo { get; set; }
    }
}

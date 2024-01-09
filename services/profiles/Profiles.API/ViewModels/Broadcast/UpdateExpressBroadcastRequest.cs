using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Broadcast
{
    public class UpdateExpressBroadcastRequest
    {
        public List<VehicleModel> Vehicles { get; set; }
        public int OrderId { get; set; }
    }
}

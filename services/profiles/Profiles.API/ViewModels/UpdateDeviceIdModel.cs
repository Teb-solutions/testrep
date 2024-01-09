using EasyGas.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class UpdateDeviceIdModel
    {
        public int? UserId { get; set; }
        public string NewDeviceId { get; set; }
        public string OldDeviceId { get; set; }
        public Source Source { get; set; }
    }
}

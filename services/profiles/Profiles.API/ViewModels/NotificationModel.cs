using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public string Timestamp { get; set; }
    }
}

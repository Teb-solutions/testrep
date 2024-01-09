using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class CustomerNotificationModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Frequency { get; set; }
        public string CreatedBy { get; set; }

    }
}

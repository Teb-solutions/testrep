using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class DistributorModel
    {
        public UserAndProfileModel UserProfile { get; set; }
        public AddressModel Address { get; set; }
    }
}

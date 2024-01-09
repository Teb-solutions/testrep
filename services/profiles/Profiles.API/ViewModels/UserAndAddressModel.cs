using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class UserAndAddressModel
    {
        public int UserId { get; set; }
        public AddressModel Address { get; set; }
    }
}

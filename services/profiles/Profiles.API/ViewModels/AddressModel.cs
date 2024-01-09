using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class AddressModel
    {
        public int? TenantId { get; set; }
        public int? BranchId { get; set; }
        public int UserAddressId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Details { get; set; }
        public string Landmark { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string PhoneAlternate { get; set; }
    }
}

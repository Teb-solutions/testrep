using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Pincode
{
    public class CreatePincodeRequest
    {
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public string Code { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
    }
}

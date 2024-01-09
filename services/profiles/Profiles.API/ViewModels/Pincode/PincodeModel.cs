using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Pincode
{
    public class PincodeModel : Trackable
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public string Code { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public bool IsActive { get; set; }
        public string DisabledBy { get; set; }
        public string EnabledBy { get; set; }
    }
}

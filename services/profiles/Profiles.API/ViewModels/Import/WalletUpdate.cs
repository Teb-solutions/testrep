using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Import
{
    public class WalletUpdate
    {
        public string MobileNumber { get; set; }
        public string TenantUID { get; set; }
        public int NewUniqueId { get; set; }
        public WalletType WalletType { get; set; }
    }
}

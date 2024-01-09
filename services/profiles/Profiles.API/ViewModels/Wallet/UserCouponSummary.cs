using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Wallet
{
    public class UserCouponSummary
    {
        public decimal Used { get; set; }
        public decimal Alloted { get; set; }
        public decimal Redeem { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Wallet
{
    public class CustomerRewardsModel
    {
        public string MyReferralCode { get; set; }
        public int UsedPoints { get; set; }
        public int AllotedPoints { get; set; }
        public int ActivePoints { get; set; }
        public int TotalPoints => UsedPoints + AllotedPoints + ActivePoints;

        public string OfferTitle { get; set; }
        public string OfferSummary { get; set; }
        public string ShareTitle { get; set; }
        public string ShareSummary { get; set; }
        public string OfferRules { get; set; }
    }
}

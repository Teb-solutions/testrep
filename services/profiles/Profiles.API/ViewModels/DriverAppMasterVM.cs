using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class DriverAppMasterVM
    {

        public string CallCenterNumber { get; set; }
        public string PaymentWebViewUrl { get; set; }
        public List<FeedbackModel> OrderCancelComboList { get; set; }
        public List<FeedbackModel> OrderFeedbackComboList { get; set; }

    }
}

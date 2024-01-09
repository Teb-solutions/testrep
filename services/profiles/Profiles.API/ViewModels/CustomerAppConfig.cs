using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class CustomerAppConfigRequest
    {
        public string DeviceId { get; set; }
    }

    public class CustomerAppConfig
    {
        public List<string> HomepageSlideshowImageList { get; set; }
        public List<BranchModel> CityList { get; set; }
        public List<string> OrderCancelComboList { get; set; }
        public List<string> OrderFeedbackComboList { get; set; }
        public List<string> SurrenderFeedbackComboList { get; set; }
    }
}

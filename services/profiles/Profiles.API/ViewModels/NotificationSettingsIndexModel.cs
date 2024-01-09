using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class NotificationSettingsIndexModel
    {
        public List<NotificationSettingsDetails> SettingsList { get; set; }
    }

    public class NotificationSettingsDetails
    {
        public NotificationSettings Settings { get; set; }
        public NotificationAnalytics Analytics { get; set; }
    }

    public class NotificationAnalytics
    {
        public int TotalSent{ get; set; }
        public int TotalReceived { get; set; }
        public int TotalDismissed { get; set; }
        public int TotalOpened { get; set; }
    }
}

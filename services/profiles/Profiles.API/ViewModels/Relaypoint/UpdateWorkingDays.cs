using System;
using System.Collections.Generic;

namespace Profiles.API.ViewModels.Relaypoint
{
    public class UpdateWorkingDaysRequest
    {
        public List<WorkingDaysModel> WorkingDaysList { get; set; }
    }

    public class WorkingDaysModel
    {
        public DayOfWeek Day { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateWorkingTimeRequest
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}

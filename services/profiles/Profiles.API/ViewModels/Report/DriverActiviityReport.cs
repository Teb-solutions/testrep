using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Report
{
    public class DriverActiviityReport
    {
        public List<DriverActiviitySummary> ActivitySummaryList { get; set; }
    }

    public class DriverActiviitySummary
    {
        public DateTime? ActivityStartTime { get; set; }
        public DateTime? ActivityEndTime { get; set; }
        public long TotalActiveTimeInMin { get; set; }
        public long TotalDistanceTravelledInMet { get; set; }
        public long TotalBreakTimeInMin { get; set; }
        public int TotalBreaks { get; set; }

        public List<DriverActivityModel> ActivityList { get; set; }
    }

    public class DriverActivityModel
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleRegNo { get; set; }
        public DLoginState LoginState { get; set; }
        public DActivityState ActivityState { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

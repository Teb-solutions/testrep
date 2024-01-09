using Profiles.API.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class DriverModel
    {
        public UserAndProfileModel UserProfile { get; set; }
    }

    public class DriverActivityOrderSummaryVM
    {
        public List<DriverOrderSummaryVM> OrderSummaryList { get; set; }
        public IEnumerable<DriverActivity> ActivityList { get; set; }
    }
    public class DriverOrderSummaryVM
    {
        public DateTime Date { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string TenantName { get; set; }
        public string BranchName { get; set; }
        public int OrdersDelivered { get; set; }
        public int OrdersDeliveredDelayed { get; set; }
        public int OrdersMissed { get; set; }
        public int OrdersPendingDelayed { get; set; }
        public int OrdersPendingNotDelayed { get; set; }
        public int OrdersCancelled { get; set; }
        public long TravelledMeters { get; set; }
        public long ActiveTimeInMin { get; set; }
        public float RewardPoints { get; set; }

        //universally accepted time format 
        public long StartPeriodUAT { get; set; }
        public long EndPeriodUAT { get; set; }

        public float CashCollected { get; set; }
        public float DaysWorked { get; set; }
        public DLoginState Dls { get; set; }
        public DateTime DlsTime { get; set; }
        public int DlsTimeAgo { get; set; }
        public DActivityState Das { get; set; }
        public DateTime DasTime { get; set; }
        public int DasTimeAgo { get; set; }
        public DriverActivitySummaryVM DriverActivitySummary {get; set;}
    }

    public class DriverActivitySummaryVM
    {
        public List<DriverActivityModel> ActivityList { get; set; }
        public long ActiveTimeInMin { get; set; }
        public long InActiveTimeInMin { get; set; }
        public List<DriverLoginStateVM> LoginStateSummaryList { get; set; }
        public DateTime WorkStartTime { get; set; }
        public DateTime? WorkEndTime { get; set; }
    }

    public class DriverLoginStateVM
    {
        public DLoginState LoginState { get; set; }
        public long Count { get; set; }
        public long DurationInMin { get; set; }
    }

    public class DriverActivityRequest
    {
        public DLoginState DriverLoginState { get; set; }
        public DActivityState DriverActivityState { get; set; }
        public double VehicleLat { get; set; }
        public double VehicleLng { get; set; }
    }

    public class DriverActivityResponse
    {
        public DLoginState DriverLoginState { get; set; }
        public DActivityState DriverActivityState { get; set; }
        public VehicleState VehicleState { get; set; }
    }
}

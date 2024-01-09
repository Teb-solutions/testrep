using System;

namespace Profiles.API.ViewModels.Analytics
{
    public class CustomerOrderAnalytics
    {
        public int CustomersRegistered { get; set; }
        public int CustomersOrdered { get; set; }
    }

    public class CustomerOrderAnalyticsPeriodwise
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public CustomerOrderAnalytics Analytics { get; set; }
    }

    public class CustomerBasicDetails
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Mobile { get; set; }
        public DateTime? LastOrderedAt { get; set; }
    }
}

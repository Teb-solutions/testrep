using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models.AdminWebsiteVM
{
    public class PWCustomerVM
    {
        public List<UserAndProfileModel> CustomerList { get; set; }
        public int InstalledNotRegisteredCustomers { get; set; }
        public List<BusinessEntityModel> DistributorList { get; set; }
    }

    public class PWDriverVM
    {
        public List<DriverModel> DriverList { get; set; }
    }

    public class PWDistributorVM
    {
        public List<DistributorModel> DistributorList { get; set; }
    }
}

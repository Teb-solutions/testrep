using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using System.Collections.Generic;

namespace Profiles.API.ViewModels.Crm
{
    public class CreateCrmStaffRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
        public List<PulzProduct> PulzProducts { get; set; }

    }
}

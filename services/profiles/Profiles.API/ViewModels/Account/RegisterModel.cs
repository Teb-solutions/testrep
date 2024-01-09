using EasyGas.Shared;
using EasyGas.Shared.Enums;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Account
{
    public class RegisterModel
    {
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public int? BusinessEntityId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public UserType Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string RoleName { get; set; }
        public CreationType CreationType { get; set; }
        public Source? Source { get; set; }
    }
}

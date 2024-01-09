using EasyGas.Shared.Enums;

namespace Profiles.API.ViewModels.Staff
{
    public class AddStaffRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public UserType UserType { get; set; }
    }
}

using System.Collections.Generic;

namespace EasyGas.Services.Profiles.Models
{
    public class UserDetailsModel
    {
        public UserAndProfileModel UserAndProfile { get; set; }
        public List<AddressModel> AddressList { get; set; }
    }
}

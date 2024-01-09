using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class LoginByOTPRequest
    {
        [Required]
        public string Mobile { get; set; }
        public string DeviceId { get; set; }
    }

    public class LoginByOTPResponse
    {
        public string Mobile { get; set; }
        public string OTPUniqueId { get; set; }
        public bool IsNewUser { get; set; }
    }

    public class LoginByOTPResendRequest
    {
        [Required]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Invalid request")]
        public string OTPUniqueId { get; set; }
    }

    public class LoginByOTPResendResponse
    {
        public string Mobile { get; set; }
        public string OTPUniqueId { get; set; }
    }
}

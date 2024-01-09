using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class UpdateMobileGetOTPRequest
    {
        [Required]
        public string Mobile { get; set; }
    }

    public class UpdateMobileGetOTPResponse
    {
        public string Mobile { get; set; }
        public string OTPUniqueId { get; set; }
    }

    public class UpdateMobileValidateOTPRequest
    {
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string OTPUniqueId { get; set; }
        [Required]
        public string OTPValue { get; set; }
    }

    public class UpdateMobileValidateOTPResponse
    {
        public string Mobile { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class LoginValidateOTPRequest
    {
        [Required]
        public string Mobile { get; set; }
        public string ReferralCode { get; set; }
        [Required]
        public string OTPUniqueId { get; set; }
        [Required]
        public string OTPValue { get; set; }
        public string TempUserToken { get; set; }
    }
}

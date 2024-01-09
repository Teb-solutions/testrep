using EasyGas.Shared;
using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }
        public string Credentials { get; set; }
        public string GrantType { get; set; }
        public string DeviceId { get; set; }
        public UserType? UserType { get; set; }
        public const string PasswordGrantType = "password";
        public const string OtpGrantType = "otp";
        public Source? Source { get; set; }
    }

    public class LoginByPasswordModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }
}

using EasyGas.Shared;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Models
{
    public class UserLogin : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public Source? Source { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }

        public UserLogin(Source? source, string deviceId, string ipAddress)
        {
            Source = source;
            DeviceId = deviceId;
            IpAddress = ipAddress;
        }
    }
}

using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.Complaint
{
    public class NewCustomerComplaint
    {
        [Required]
        [StringLength(200)]
        public string Subject { get; set; }
        [Required]
        [StringLength(1000)]
        public string Message { get; set; }
        public string AttachmentBase64 { get; set; }
        [Required]
        public ComplaintCategory Category { get; set; }
    }
}

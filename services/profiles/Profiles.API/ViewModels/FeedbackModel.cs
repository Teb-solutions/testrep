using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class FeedbackModel
    {
        public int FeedbackId { get; set; }
        public int? BranchId { get; set; }
        public int TenantId { get; set; }
        public FeedbackType FeedbackType { get; set; }
        public LanguageType Language { get; set; }
        public string Remarks { get; set; }
    }
}

using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Formatters;
using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class CustomerComplaintModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string AttachmentUrl { get; set; }
        public ComplaintCategory Category { get; set; }
        public string CategoryName { get; set; }
        public string Remarks { get; set; }

        public ComplaintStatus Status { get; set; }
        public string StatusName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? ReOpenAt { get; set; }

        public static CustomerComplaintModel FromComplaint(Profiles.API.Models.Complaint complaint, string storageUrl)
        {
            return new CustomerComplaintModel()
            {
                Id = complaint.Id,
                Code = "#" + complaint.Id.ToString().PadLeft(4, '0'),
                Category = complaint.Category,
                CategoryName = complaint.Category.ToString(),
                AttachmentUrl = string.IsNullOrEmpty(complaint.AttachmentUrl) ? "" : storageUrl + "/" + complaint.AttachmentUrl,
                Message = complaint.Message,
                Subject = complaint.Subject,
                Remarks = complaint.Remarks,
                ClosedAt = complaint.ClosedAt,
                CreatedAt = complaint.CreatedAt,
                ReOpenAt = complaint.ReOpenAt,
                Status = complaint.Status,
                StatusName = EnumHelper<ComplaintStatus>.GetDisplayDescription(complaint.Status)
            };
        }
    }
}

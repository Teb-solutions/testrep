using EasyGas.Services.Profiles.Models;
using Profiles.API.Models;
using System;

namespace Profiles.API.ViewModels.Complaint
{
    public class CrmTicketModel : Trackable
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int TenantId { get; set; }
        public int? BranchId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string AttachmentUrl { get; set; }
        public ComplaintCategory Category { get; set; }

        public string Remarks { get; set; }

        public ComplaintStatus Status { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        public string ClosedByUserFullName { get; set; }

        public DateTime? ReOpenAt { get; set; }
        public int? ReOpenByUserId { get; set; }
        public string ReOpenByUserFullName { get; set; }
        public string ReOpenReason { get; set; }

        public static CrmTicketModel FromComplaint(Profiles.API.Models.Complaint complaint, string storageUrl)
        {
            CrmTicketModel complaintModel = new CrmTicketModel();
            complaintModel.Id = complaint.Id;
            complaintModel.Code = "#" + complaint.Id.ToString().PadLeft(4, '0');
            complaintModel.BranchId = complaint.BranchId;
            complaintModel.TenantId = complaint.TenantId;
            complaintModel.UserId = complaint.UserId;
            complaintModel.UserFullName = complaint.User.Profile.GetFullName();
            complaintModel.UserMobile = complaint.User.Profile.Mobile;
            complaintModel.AttachmentUrl = string.IsNullOrEmpty(complaint.AttachmentUrl) ? "" : storageUrl + "/" + complaint.AttachmentUrl;
            complaintModel.Subject = complaint.Subject;
            complaintModel.Message = complaint.Message;
            complaintModel.Category = complaint.Category;
            complaintModel.Remarks = complaint.Remarks;
            complaintModel.Status = complaint.Status;

            complaintModel.ClosedByUserId = complaint.ClosedByUserId;
            complaintModel.ClosedByUserFullName = complaint.ClosedByUser?.Profile.GetFullName();
            complaintModel.ClosedAt = complaint.ClosedAt;

            complaintModel.ReOpenByUserId = complaint.ReOpenByUserId;
            complaintModel.ReOpenByUserFullName = complaint.ReOpenByUser?.Profile.GetFullName();
            complaintModel.ReOpenAt = complaint.ReOpenAt;
            complaintModel.ReOpenReason = complaint.ReOpenReason;

            complaintModel.CreatedAt = complaint.CreatedAt;

            return complaintModel;
        }
    }

    public class CloseCrmTicketRequest
    {
        public string Remarks { get; set; }
    }

    public class ReopenCrmTicketRequest
    {
        public string Reason { get; set; }
    }
}

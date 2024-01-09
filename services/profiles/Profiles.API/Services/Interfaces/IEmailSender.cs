using Profiles.API.ViewModels.Complaint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string receipientEmail, string receipientName, string subject, string body, List<string> attachments);
        Task<bool> SendEmailToAdminForNewComplaint(int complaintId);
        Task<bool> SendEmailToAdminForNewOrder(int userId, string deliverySlotname, int branchId);
    }
}

using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);

        Task<bool> SendSmsToCustomerForOrderConfirmation(int userId);
        Task<bool> SendSmsToCustomerForOrderDispatch(int userId, string otp);
        Task<bool> SendSmsToCustomerForOrderDelivered(int userId, string orderCode, string invoiceNo, string invoiceUrl);

        Task<bool> SendSmsToCustomerForRegisterOtp(string customerPhone, string customerName, string otp, int tenantId);
        Task<bool> SendSmsToCustomerForResetPasswordOtp(UserAndProfileModel userProfile, string otp);
        Task<bool> SendSmsToCustomerForLoginOtp(UserAndProfileModel userProfile, string otp);
        Task<bool> SendSmsToCustomerForChangeMobileOtp(UserAndProfileModel userProfile, string otp);

        Task<bool> SendSmsToDriverForRelaypointOrderConfirmation(int userId, string orderCode, string otp);
    }
}

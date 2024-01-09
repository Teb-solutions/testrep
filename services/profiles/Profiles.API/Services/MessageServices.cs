using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EasyGas.Services.Profiles.Models.MessageRequest;
using System.Net.Http;
using Newtonsoft.Json;
using EasyGas.Services.Profiles.Queries;
using Profiles.API.Models;
using Profiles.API.ViewModels.Complaint;
using Microsoft.Extensions.Logging;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;

namespace EasyGas.Services.Profiles.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _db;
        static string _smsUrl = "";
        private readonly string excelUrl;
        private readonly ILogger<AuthMessageSender> _logger;

        public AuthMessageSender(IOptions<ApiSettings> apiSettings, ProfilesDbContext db, IConfiguration cfg, ILogger<AuthMessageSender> logger)
        {
            _apiSettings = apiSettings;
            _db = db;
            _smsUrl = _apiSettings.Value.SmsApiUrl + _apiSettings.Value.SmsUserCred + "&state=4&receipientno={customerPhone}&msgtxt=";
            excelUrl = cfg["ReportExcelUrl"];
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string receipientEmail, string receipientName, string subject, string body, List<string> attachments)
        {
            try
            {
                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = receipientName,
                    RecipientEmail = receipientEmail,
                    Type = MessageType.EMAIL,
                    MessageHeader = subject,
                    content = new
                    {
                        Body = body
                    },
                };
                if (attachments.Count > 0)
                {
                    messageModel.AttachmentUrls = attachments;
                }
                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}SendEmail", _apiSettings.Value.BroadcastServiceUrl);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var emailJson = JsonConvert.SerializeObject(messageModel);
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return true;
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            return Task.FromResult(0);
        }

        
        public async Task<bool> SendSmsToCustomerForOrderConfirmation(int userId)
        {
            User user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return false;
            }
          
            bool sendSms = false;
            string phone = user.Profile.Mobile;

            try
            {
                #region update last order date

                user.Profile.LastOrderedAt = DateMgr.GetCurrentIndiaTime();
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                #endregion

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = user.Profile.GetFullName(),
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new { }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderCreationAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToCustomerForOrderDispatch(int userId, string deliveryOtp)
        {
            User user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(deliveryOtp))
            {
                return false;
            }
            bool sendSms = false;

            string phone = user.Profile.Mobile;

            try
            {

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = user.Profile.GetFullName(),
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = deliveryOtp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderDispatchAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToDriverForRelaypointOrderConfirmation(int userId, string orderCode, string otp)
        {
            User user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(otp))
            {
                return false;
            }
            bool sendSms = false;

            string phone = user.Profile.Mobile;

            try
            {

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = user.Profile.GetFullName(),
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = otp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderPickupAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToCustomerForOrderDelivered(int userId, string orderCode, string invoiceNo, string invoiceUrl)
        {
            User user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return false;
            }

            bool sendSms = false;
            string phone = user.Profile.Mobile;
            //strFinal = strData.Replace("{customer}", customerName);
            try
            {
                #region update last order delivery date

                user.Profile.LastOrderDeliveredAt = DateMgr.GetCurrentIndiaTime();
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                #endregion

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = user.Profile.GetFullName(),
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        UserName = user.Profile.GetFullName(),
                        OrderCode = orderCode,
                        InvoiceNo = invoiceNo,
                        Link = invoiceUrl
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderDeliveredAsync", _apiSettings.Value.BroadcastServiceUrl);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    //var emailJson = JsonConvert.SerializeObject(messageModel);
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;

        }

        /*
        public async Task<bool> SendSmsDeliveryOrderCode(string phone, string deliveryCode, string customerName)
        {
            if (string.IsNullOrEmpty(deliveryCode))
            {
                return false;
            }
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }

            bool sendSms = false;

            try
            {

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = customerName,
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = deliveryCode
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderDispatchAsync", _apiSettings.Value.BroadcastSmsUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }
        */

        public async Task<bool> SendSmsToCustomerForRegisterOtp(string customerPhone, string customerName, string otp, int tenantId)
        {
            bool sendSms = false;
            string phone = customerPhone;
            /*
            string smsUrl = _smsUrl;
            smsUrl = smsUrl.Replace("{customerPhone}", phone);
            string smsContent = string.Empty;
            string strFinal = string.Empty;
            smsContent = _apiSettings.Value.SmsTRegisterOtp;
            smsContent = smsContent.Replace("{OTP}", otp);
            //strFinal = strData.Replace("{customer}", customerName);
            */
            try
            {
                /*
                smsUrl += smsContent;
                sendSms = SendSMS(smsUrl);
                SmsLog smsLog = new SmsLog
                {
                    TenantId = tenantId,
                    IsSuccess = sendSms,
                    Phone = phone,
                    Content = smsContent,
                    
                };
                _db.SmsLogs.Add(smsLog);
                _db.SaveChanges();
                */

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = customerName,
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = otp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}RegisterOtpAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical("SendSmsToCustomerForRegisterOtp Catch | Not able to send the message because of " + e.Message);
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToCustomerForResetPasswordOtp(UserAndProfileModel userProfile, string otp)
        {
            bool sendSms = false;
            string phone = userProfile.Mobile;
            /*
            string smsUrl = _smsUrl;
            smsUrl = smsUrl.Replace("{customerPhone}", phone);
            string smsContent = string.Empty;
            string strFinal = string.Empty;
            smsContent = _apiSettings.Value.SmsTResetPasswordOtp;
            smsContent = smsContent.Replace("{OTP}", otp);
            //strFinal = strData.Replace("{customer}", customerName);
            */
            try
            {
                /*
                smsUrl += smsContent;
                sendSms = SendSMS(smsUrl);
                SmsLog smsLog = new SmsLog
                {
                    TenantId = userProfile.TenantId,
                    IsSuccess = sendSms,
                    Phone = phone,
                    Content = smsContent,
                    UserId = userProfile.UserId
                };
                _db.SmsLogs.Add(smsLog);
                _db.SaveChanges();
                */

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = userProfile.FirstName,
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = otp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}ResetPasswordOtpAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToCustomerForLoginOtp(UserAndProfileModel userProfile, string otp)
        {
            bool sendSms = false;
            string phone = userProfile.Mobile;
            /*
            string smsUrl = _smsUrl;
            smsUrl = smsUrl.Replace("{customerPhone}", phone);
            string smsContent = string.Empty;
            string strFinal = string.Empty;
            smsContent = _apiSettings.Value.SmsTLoginOtp; //TODO change template
            smsContent = smsContent.Replace("{OTP}", otp);
            //strFinal = strData.Replace("{customer}", customerName);
            */
            try
            {
                /*
                smsUrl += smsContent;
                sendSms = SendSMS(smsUrl);
                SmsLog smsLog = new SmsLog
                {
                    TenantId = userProfile.TenantId,
                    IsSuccess = sendSms,
                    Phone = phone,
                    Content = smsContent,
                    UserId = userProfile.UserId
                };
                _db.SmsLogs.Add(smsLog);
                _db.SaveChanges();
                */

                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = userProfile.FirstName,
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = otp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}LoginOtpAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

        public async Task<bool> SendSmsToCustomerForChangeMobileOtp(UserAndProfileModel userProfile, string otp)
        {
            bool sendSms = false;
            string phone = userProfile.Mobile;
            /*
             * string smsUrl = _smsUrl;
            smsUrl = smsUrl.Replace("{customerPhone}", phone);
            string smsContent = _apiSettings.Value.SmsTRegisterOtp;
            smsContent = smsContent.Replace("{OTP}", otp);
            //strFinal = strData.Replace("{customer}", customerName);
            */
            try
            {
                /*
                smsUrl += smsContent;
                sendSms = SendSMS(smsUrl);
                SmsLog smsLog = new SmsLog
                {
                    TenantId = userProfile.TenantId,
                    IsSuccess = sendSms,
                    Phone = phone,
                    Content = smsContent,
                    UserId = userProfile.UserId
                };
                _db.SmsLogs.Add(smsLog);
                _db.SaveChanges();
                */
                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = userProfile.FirstName,
                    RecipientNumber = phone,
                    Type = MessageType.SMS,
                    content = new
                    {
                        OTP = otp
                    }
                };
                //messageModel.RecipientNumber = "9400407370"; // for testing

                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}RegisterOtpAsync", _apiSettings.Value.BroadcastServiceUrl);
                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        sendSms = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + e.Message);
                //return false;
            }
            return sendSms;
        }

       
        public static bool SendSMS(string SMSstring)
        {
            bool bScuccess = true;
            try
            {
                string dataString = null;
                WebRequest request = HttpWebRequest.Create(SMSstring);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();
                StreamReader readStream = new StreamReader(s);
                dataString = readStream.ReadToEnd();
                string str = dataString;
                string value = dataString.Substring(7, 1);
                int retV = 100;
                int.TryParse(value, out retV);
                bScuccess = (retV == 0) ? true : false;
                response.Close();
                s.Close();
                readStream.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                //throw;
            }
            return bScuccess;
        }

        public async Task<bool> SendOrdersEmailToRelaypointAsync(string relapointName, string relapointEmail, string attachmentUrl, string subject, string body)
        {
            try
            {
                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = relapointName,
                    RecipientEmail = relapointEmail,
                    Type = MessageType.EMAIL,
                    MessageHeader = subject,
                    content = new
                    {
                        Body = body
                    },
                    AttachmentUrls = new List<string>()
                    {
                        new string(attachmentUrl)
                    }
                };
                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}SendEmail", _apiSettings.Value.BroadcastServiceUrl);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var emailJson = JsonConvert.SerializeObject(messageModel);
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return true;
                    }

                }
                return false;
            }
            catch (ArgumentNullException nullEx)
            {
                System.Diagnostics.Trace.TraceError(nullEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + ex.Message);
                return false;
            }
        }
        public async Task<bool> SendEmailToAdminForNewComplaint(int complaintId)
        {
            var complaint = _db.Complaints.Include(p => p.User.Profile).Where(p => p.Id == complaintId).FirstOrDefault();
            if (complaint == null)
            {
                return false;
            }            

            var toEmail = _apiSettings.Value.OrderAlertToEmail;

            try
            {
                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = "Admin",
                    RecipientEmail = toEmail,
                    Type = MessageType.EMAIL,
                    MessageHeader = "Easygas Customer Message Alert",
                    AttachmentUrls = string.IsNullOrEmpty(complaint.AttachmentUrl) ? new List<string> { complaint.AttachmentUrl} : new List<string>(), 
                    content = new
                    {
                        CustomerName = complaint.User.Profile.GetFullName(),
                        CustomerMobile = complaint.User.Profile.Mobile,
                        Category = complaint.Category,
                        Subject = complaint.Subject,
                        Message = complaint.Message,
                    },
                };
                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}ComplaintCreatedEmail", _apiSettings.Value.BroadcastServiceUrl);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var emailJson = JsonConvert.SerializeObject(messageModel);
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return true;
                    }

                }
                return false;
            }
            catch (ArgumentNullException nullEx)
            {
                System.Diagnostics.Trace.TraceError(nullEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + ex.Message);
                return false;
            }
        }

        
        public async Task<bool> SendEmailToAdminForNewOrder(int userId, string deliverySlotname, int branchId)
        {
            var toEmail = _apiSettings.Value.OrderAlertToEmail;
            User user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
            Branch branch = _db.Branches.Where(p => p.Id == branchId).FirstOrDefault();
            try
            {
                RequestModel messageModel = new RequestModel()
                {
                    RecipientName = "Admin",
                    RecipientEmail = toEmail,
                    Type = MessageType.EMAIL,
                    MessageHeader = "New " + branch?.Name + " " + deliverySlotname + " EasyGasIndia Order Alert",
                    content = new
                    {
                        CustomerName = string.IsNullOrEmpty(user.Profile.GetFullName()) ? user.Profile.Mobile : user.Profile.GetFullName(),
                        ConnectionType = "",
                        DeliverySlot = deliverySlotname,
                        BranchName = branch?.Name
                    },
                };
                using (HttpClient _apiClient = new HttpClient())
                {
                    var uri = string.Format("{0}OrderCreatedEmail", _apiSettings.Value.BroadcastServiceUrl);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var emailJson = JsonConvert.SerializeObject(messageModel);
                    var response = await _apiClient.PostAsJsonAsync(uri, messageModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return true;
                    }

                }
                return false;
            }
            catch (ArgumentNullException nullEx)
            {
                System.Diagnostics.Trace.TraceError(nullEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Not able to send the message because of " + ex.Message);
                return false;
            }
        }
        
    }
 }

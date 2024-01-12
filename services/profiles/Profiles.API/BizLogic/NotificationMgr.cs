using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using EasyGas.Services.Core.Commands;
using Microsoft.Extensions.Configuration;

using EasyGas.Shared;
using EasyGas.Shared.Formatters;
using EasyGas.Shared.Enums;
using Azure.Storage.Blobs;
using Profiles.API.ViewModels.Notification;
using EasyGas.Shared.Models;
using Profiles.API.IntegrationEvents;
using EasyGas.BuildingBlocks.EventBus.Events;
using Profiles.API.ViewModels;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class NotificationMgr
    {
        private readonly ProfilesDbContext _db;
        private readonly ILogger _logger;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IProfilesIntegrationEventService _profilesIntegrationEventService;

        public NotificationMgr(ProfilesDbContext db, IOptions<ApiSettings> apiSettings,
            BlobServiceClient blobServiceClient, IProfilesIntegrationEventService profilesIntegrationEventService,
            ILoggerFactory loggerFactory)
        {
            _db = db;
            _apiSettings = apiSettings;
            _blobServiceClient = blobServiceClient;
            _profilesIntegrationEventService = profilesIntegrationEventService;
            _logger = loggerFactory.CreateLogger<NotificationMgr>();
        }

        

        public async Task<CommandResult> GetLoginPopupNotification()
        {
            List<string> validationErrors = new List<string>();

                var notiSettings = _db.NotificationSettings.Where(p => p.NotiTriggerType == NotificationTriggerType.APP_LOGIN_POPUP && p.IsActive == true).FirstOrDefault();
                if (notiSettings != null)
                {
                    if (!notiSettings.IsWebview && string.IsNullOrEmpty(notiSettings.Description))
                    {
                        validationErrors.Add("No popup notification found");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                    else if (notiSettings.IsWebview && string.IsNullOrEmpty(notiSettings.WebviewUrl))
                    {
                        validationErrors.Add("No popup notification found");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }

                    return new CommandResult(System.Net.HttpStatusCode.OK, notiSettings);
                }

            validationErrors.Add("No popup notification found");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        public async Task<NotificationSettingsIndexModel> GetAllNotificationSettings(int tenantId, int? branchId)
        {
            NotificationSettingsIndexModel model = new NotificationSettingsIndexModel() 
            {
                SettingsList = new List<NotificationSettingsDetails>()
            };

                var notiSettings = await _db.NotificationSettings.Where(p => p.TenantId == tenantId && p.NotiTriggerType != NotificationTriggerType.APP_LOGIN_POPUP).ToListAsync();
                if (branchId != null)
                {
                    notiSettings = notiSettings.Where(p => p.BranchId == branchId).ToList();
                }
                foreach(var p in notiSettings)
                {
                    NotificationSettingsDetails details = new NotificationSettingsDetails()
                    {
                        Settings = p,
                        Analytics = new NotificationAnalytics()
                    };
                    //TODO recheck for branch
                    int totSent = _db.Notifications.Where(query => query.Description == p.Description && query.UserCategory == p.NotiUserCategory).Count();
                    details.Analytics.TotalSent = totSent;

                    model.SettingsList.Add(details);
                }

            return model;
        }

        public async Task<CommandResult> CreateNotificationSettings(NotificationSettings notiSettingModel)
        {
            List<string> validationErrors = new List<string>();

                if (notiSettingModel.NotiTriggerType == NotificationTriggerType.AUTO_SEND_FREQUENCY)
                {
                    if (notiSettingModel.Frequency <= 0 || notiSettingModel.Frequency == null)
                    {
                        validationErrors.Add("Frequency is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                }
                else if (notiSettingModel.NotiTriggerType == NotificationTriggerType.AUTO_SEND_SCHEDULED)
                {
                    if (notiSettingModel.ScheduledDate == null)
                    {
                        validationErrors.Add("Scheduled Date is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                }

                if (notiSettingModel.NotiTriggerTime == NotificationTriggerTime.FIXED)
                {
                    if (notiSettingModel.FromTime == null)
                    {
                        validationErrors.Add("Time is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                }

                if (string.IsNullOrEmpty(notiSettingModel.Description))
                {
                    validationErrors.Add("Content is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                _db.NotificationSettings.Add(notiSettingModel);
                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, notiSettingModel);

        }

        public async Task<CommandResult> UpdateNotificationSettings(NotificationSettings notiSettingModel)
        {
            List<string> validationErrors = new List<string>();

                NotificationSettings notiSetting = _db.NotificationSettings.Where(p => p.Id == notiSettingModel.Id).FirstOrDefault();
                if (notiSetting == null)
                {
                    validationErrors.Add("Not found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                notiSetting.NotiTriggerType = notiSettingModel.NotiTriggerType;
                if (notiSettingModel.NotiTriggerType == NotificationTriggerType.AUTO_SEND_FREQUENCY)
                {
                    if (notiSettingModel.Frequency <= 0 || notiSettingModel.Frequency == null)
                    {
                        validationErrors.Add("Frequency is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                    notiSetting.Frequency = notiSettingModel.Frequency;
                }
                else if (notiSettingModel.NotiTriggerType == NotificationTriggerType.AUTO_SEND_SCHEDULED)
                {
                    if (notiSettingModel.ScheduledDate == null)
                    {
                        validationErrors.Add("Scheduled Date is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                    notiSetting.ScheduledDate = notiSettingModel.ScheduledDate;
                }

                notiSetting.NotiTriggerTime = notiSettingModel.NotiTriggerTime;
                if (notiSettingModel.NotiTriggerTime == NotificationTriggerTime.FIXED)
                {
                    if (notiSettingModel.FromTime == null)
                    {
                        validationErrors.Add("Time is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                    notiSetting.FromTime = notiSettingModel.FromTime;
                }

                if (string.IsNullOrEmpty(notiSettingModel.Title))
                {
                    validationErrors.Add("Title is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                if (string.IsNullOrEmpty(notiSettingModel.Description))
                {
                        validationErrors.Add("Content is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                notiSetting.Title = notiSettingModel.Title;
                notiSetting.Description = notiSettingModel.Description;
                notiSetting.IsActive = notiSettingModel.IsActive;

                if (notiSettingModel.IsImageActive && !string.IsNullOrEmpty(notiSettingModel.ImageBase64String) && !string.IsNullOrEmpty(notiSettingModel.ImageExtension))
                {
                    var imageData = GetImageData(notiSettingModel.ImageBase64String);
                    if (imageData != null)
                    {
                        string fileName = "noti" + Guid.NewGuid().ToString() + notiSettingModel.ImageExtension;
                        var isSaved = await SaveDataToBlob(imageData, fileName, _apiSettings.Value.BlobCustomerNotificationsContainer);
                        if (isSaved)
                        {
                            string storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerNotificationsContainer;
                            notiSetting.Imageurl = storageUrl + "/" + fileName;
                        }
                    }
                }

                if (notiSettingModel.IsImageActive && !string.IsNullOrEmpty(notiSetting.Imageurl))
                {
                    notiSetting.IsImageActive = true;
                }
                else
                {
                    notiSetting.IsImageActive = false;
                }

                _db.Entry(notiSetting).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, notiSetting);
        }

        private static byte[] GetImageData(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private async Task<bool> SaveDataToBlob(byte[] imgData, string fileName, string containerName)
        {
            try
            {
                var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                var blobClient = blobContainer.GetBlobClient(fileName);

                await blobClient.DeleteIfExistsAsync();

                using (var stream = new MemoryStream(imgData))
                {
                    await blobClient.UploadAsync(stream);
                    return true;
                }
            }
            catch (Exception)
            {
                
            }
            return false;
        }

        public async Task<CommandResult> SendTestNotification(NotificationSettings setting)
        {
            List<string> validationErrors = new List<string>();

                var userProfile = _db.Profiles.Include(p => p.User).Where(p => p.Mobile == setting.UserMobile && p.User.Type == UserType.CUSTOMER).FirstOrDefault();
                if (userProfile == null)
                {
                        validationErrors.Add("User with mobile " + setting.UserMobile + " is not registered in EasyGas");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (string.IsNullOrEmpty(userProfile.DeviceId))
                {
                    validationErrors.Add("User has never logged in to customer app.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                if (setting.IsImageActive)
                {
                    if (string.IsNullOrEmpty(setting.Imageurl) && !string.IsNullOrEmpty(setting.ImageBase64String) && !string.IsNullOrEmpty(setting.ImageExtension))
                    {
                        var imageData = GetImageData(setting.ImageBase64String);
                        if (imageData != null)
                        {
                            string fileName = "notitest" + DateMgr.GetCurrentIndiaTime().ToString("ddMMyyyyTHHmmss") + setting.ImageExtension;
                            var isSaved = await SaveDataToBlob(imageData, fileName, _apiSettings.Value.BlobCustomerNotificationsContainer);
                            if (isSaved)
                            {
                                string storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerNotificationsContainer;
                                setting.Imageurl = storageUrl + "/" + fileName;
                            }
                        }
                    }
                }

                var sendNoti = await AddNotification(userProfile.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_PROMOTIONS, setting.Description, PushNotificationType.DATA, setting.Title, setting.Imageurl);
                if (sendNoti)
                {
                    setting.ImageBase64String = "";
                    return new CommandResult(System.Net.HttpStatusCode.OK, setting);
                }

            validationErrors.Add("Notification could not be sent. Please try again.");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        public async Task<CommandResult> SendCustomNotification(CustomNotificationRequest request)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                if (request.IsImageActive)
                {
                    if (string.IsNullOrEmpty(request.Imageurl) && !string.IsNullOrEmpty(request.ImageBase64String) && !string.IsNullOrEmpty(request.ImageExtension))
                    {
                        var imageData = GetImageData(request.ImageBase64String);
                        if (imageData != null)
                        {
                            string fileName = "noticustom" + DateMgr.GetCurrentIndiaTime().ToString("ddMMyyyyTHHmmss") + request.ImageExtension;
                            var isSaved = await SaveDataToBlob(imageData, fileName, _apiSettings.Value.BlobCustomerNotificationsContainer);
                            if (isSaved)
                            {
                                string storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerNotificationsContainer;
                                request.Imageurl = storageUrl + "/" + fileName;
                            }
                        }
                    }
                }

                var customersCount = _db.Profiles
                    .Include(p => p.User)
                    .Where(p => p.User.Type == UserType.CUSTOMER && request.UserMobileList.Contains(p.Mobile)).Count();
                if (customersCount == 0)
                {
                    validationErrors.Add("No customers found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                    SendBulkPushNotificationEvent @event = new SendBulkPushNotificationEvent
                    {
                        Title = request.Title,
                        Description = request.Description,
                        Imageurl = request.Imageurl,
                        IsImageActive = request.IsImageActive,
                        IsUserCustomer = true,
                        UserMobileList = request.UserMobileList
                    };
                    await _profilesIntegrationEventService.PublishEventThroughEventBusAsync(@event);
                
                return new CommandResult(System.Net.HttpStatusCode.OK, new { Detail = "Notifications is being sent in the background." });
            }
            catch(Exception ex)
            {
                validationErrors.Add("Some error occurred. Please try again.");
                _logger.LogCritical("NotificationMgr.SendCustomNotification Exception | Notification " + request.Title +" exeption | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<bool> SendBulkNotification(SendBulkPushNotificationEvent @event)
        {
            int sentCount = 0;
            int totSentCount = 0;

            try
            {
                var customers = await _db.Profiles
                    .Include(p => p.User)
                    .Where(p => p.User.Type == UserType.CUSTOMER && @event.UserMobileList.Contains(p.Mobile))
                    .ToListAsync();

                customers = customers.GroupBy(p => p.Mobile).Select(p => p.First()).ToList();

                DateTime now = DateMgr.GetCurrentIndiaTime();
                foreach (var userProfile in customers)
                {
                    Notification newNoti = new Notification()
                    {
                        UserType = UserType.CUSTOMER,
                        Title = @event.Title,
                        Description = @event.Description,
                        ImageUrl = @event.IsImageActive == true ? @event.Imageurl : "",
                        GoToWebUrl = "",
                        Type = NotificationType.INFO,
                        Category = NotificationCategory.CUSTOMER_PROMOTIONS,
                        PushNotificationType = PushNotificationType.DATA,
                        UserId = userProfile.UserId,
                        DeviceId = userProfile.DeviceId,
                        Status = NotificationStatus.UNREAD,
                        CreatedBy = "",
                        UpdatedBy = "",
                        CreatedAt = now,
                        UpdatedAt = now,
                        TriggerTime = NotificationTriggerTime.ON_TRIGGER,
                        TriggerType = NotificationTriggerType.MANUAL_SEND,
                        UserCategory = NotificationUserCategory.CUSTOMER_REGISTERED
                    };

                    /*
                    if (user.UserName != "9400407370" ) //TODO remove after testing
                    {
                        return null;
                    }
                    */

                    if (!string.IsNullOrEmpty(newNoti.DeviceId))
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.CUSTOMER_APP);
                        if (notiResp != null)
                        {
                            _logger.LogInformation("NotificationMgr.SendBulkNotification {noti} sent to {user}", newNoti.Title, userProfile.Mobile);
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                            _db.Notifications.Add(newNoti);
                            sentCount += 1;
                            totSentCount += 1;
                        }
                    }

                    if (sentCount >= 100)
                    {
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("NotificationMgr.SendBulkNotification {noti} saved {count}", @event.Title, sentCount);
                        sentCount = 0;
                    }
                }

                if (sentCount > 0)
                {
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("NotificationMgr.SendBulkNotification {noti} saved {count}", @event.Title, sentCount);
                    sentCount = 0;
                }

                _logger.LogInformation("NotificationMgr.SendBulkNotification {noti} sent to {count}", @event.Title, totSentCount);

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogCritical("NotificationMgr.SendBulkNotification Exception | {noti} successfully sent to {totSentCount} customers with {@exeption}", @event.Title, totSentCount , ex);
            }

            return false;
        }

        //-------------------------------------------------------------------------------------------

        public async Task<bool> AddNotification(int userId, NotificationType type, NotificationCategory category, string desc, PushNotificationType firebaseType = PushNotificationType.NOTIFICATION, string title = "EasyGas Notification", string imageUrl = "")
        {
            var user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();

            if (user != null && !String.IsNullOrEmpty(desc))
                {
                    Notification newNoti = new Notification()
                    {
                        UserType = user.Type,
                        Title = title,
                        Description = desc,
                        ImageUrl = imageUrl,
                        Type = type,
                        Category = category,
                        PushNotificationType = firebaseType,
                        UserId = userId,
                        DeviceId = user.Profile.DeviceId,
                        Status = NotificationStatus.UNREAD,
                        CreatedBy = "", //TODO get whether from app or admin website
                        UpdatedBy = "",
                        CreatedAt = DateMgr.GetCurrentIndiaTime(),
                        UpdatedAt = DateMgr.GetCurrentIndiaTime(),
                        OrderCode = "",
                    };
                    //_db.Notifications.Add(newNoti);
                    //_db.SaveChanges();

                    if (!string.IsNullOrEmpty(newNoti.DeviceId))
                    {
                    if (user.Type == UserType.CUSTOMER)
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.CUSTOMER_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }
                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                    else if (user.Type == UserType.RELAY_POINT)
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.RELAYPOINT_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }
                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                    else
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.DRIVER_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }
                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                    }
                    else
                    {
                        return await Task.FromResult(false);
                    }
                }

            return false;
        }

        public async Task<bool> AddNotification(User user, NotificationType type, NotificationCategory category, string desc, PushNotificationType firebaseType = PushNotificationType.NOTIFICATION, string title = "EasyGas Notification", string imageUrl = "")
        {
            if (user != null && !String.IsNullOrEmpty(desc))
            {
                Notification newNoti = new Notification()
                {
                    UserType = user.Type,
                    Title = title,
                    Description = desc,
                    ImageUrl = imageUrl,
                    Type = type,
                    Category = category,
                    PushNotificationType = firebaseType,
                    UserId = user.Id,
                    DeviceId = user.Profile?.DeviceId,
                    Status = NotificationStatus.UNREAD,
                    CreatedBy = "", //TODO get whether from app or admin website
                    UpdatedBy = "",
                    CreatedAt = DateMgr.GetCurrentIndiaTime(),
                    UpdatedAt = DateMgr.GetCurrentIndiaTime(),
                    OrderCode = "",
                };
               // _db.Notifications.Add(newNoti);
                //_db.SaveChanges();

                if (!string.IsNullOrEmpty(newNoti.DeviceId))
                {
                    if (user.Type == UserType.CUSTOMER)
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.CUSTOMER_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }
                        
                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                    else if (user.Type == UserType.RELAY_POINT)
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.RELAYPOINT_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }
                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                    else 
                    {
                        var notiResp = FirebasePushNotification(newNoti, Source.DRIVER_APP);
                        if (notiResp != null)
                        {
                            newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                            newNoti.Success = notiResp.success == 1;
                            if (!newNoti.Success)
                                newNoti.Error = notiResp.results?[0].error;
                        }

                        _db.Notifications.Add(newNoti);
                        _db.SaveChanges();
                        return newNoti.Success;
                    }
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }

            return false;
        }

        public async Task<bool> AddDriverOrderAssignedNotification(List<int> driverIds)
        {
            string notification = _apiSettings.Value.DriverOrderAssignedNoti;
            bool result = true;
            foreach(var driverId in driverIds)
            {
                result = await AddNotification((int)driverId, NotificationType.INFO, NotificationCategory.DRIVER_ORDER_ASSIGNED, notification, PushNotificationType.DATA);
            }
            
            return result;
        }

        public async Task<bool> AddDriverOrderCancelledNotification(int driverId, string orderCode)
        {
            string notification = "Order #"+orderCode + " was cancelled.";
            bool result = await AddNotification(driverId, NotificationType.INFO, NotificationCategory.DRIVER_ORDER_CHANGED, notification, PushNotificationType.DATA);
            return result;
        }

        public async Task<bool> AddDriverOrderDeliveredNotification(int driverId, string orderCode)
        {
            string notification = "Order #" + orderCode + " was delivered.";
            bool result = await AddNotification(driverId, NotificationType.INFO, NotificationCategory.DRIVER_ORDER_CHANGED, notification, PushNotificationType.DATA);
            return result;
        }

        public async Task<bool> AddDriverOrderChangedNotification(List<int> driverIds)
        {
            string notification = _apiSettings.Value.DriverOrderChangedNoti;

                foreach(var driverId in driverIds)
                {
                    var result = await AddNotification((int)driverId, NotificationType.INFO, NotificationCategory.DRIVER_ORDER_CHANGED, notification, PushNotificationType.DATA);
                }
                return true;
        }

        public async Task<bool> AddRelaypointPickupOrderAssignedNotification(int relaypointId, int userId, NotificationCategory category, string orderCode)
        {
            var user = await _db.Users
                .Include(p => p.Profile)
                .Where(p => p.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return false;
            }
            string notification = "New pickup order #" + orderCode + " by ";
            notification += user.Type == UserType.DRIVER ? "rider" : "customer";
            var relaypointUser = await _db.Users
                .Include(p => p.Profile)
                .Where(p => p.BusinessEntityId == relaypointId && p.Type == UserType.RELAY_POINT)
                .FirstOrDefaultAsync();
            if (relaypointUser != null)
            {
                await AddNotification(relaypointUser, NotificationType.INFO, category, notification, PushNotificationType.DATA);
                return true;
            }

            return false;
        }

        public async Task<List<NotificationReport>> GetListByDate(DateTime date)
        {
            List<NotificationReport>  notiList = await _db.Notifications
                .Where(p => p.CreatedAt.Date == date)
                .Select(p => new NotificationReport
                {
                    Category = p.Category,
                    Description = p.Description,
                    DeviceId = p.DeviceId,
                    GoToWebUrl = p.GoToWebUrl,
                    ImageUrl = p.ImageUrl,
                    Status = p.Status,
                    Title = p.Title,
                    TriggerTime = p.TriggerTime,
                    TriggerType = p.TriggerType,
                    Type = p.Type,
                    UserCategory = p.UserCategory,
                    UserFullName = p.User.Profile.GetFullName(),
                    UserId = p.UserId,
                    UserMobile = p.User.Profile.Mobile,
                    UserType = p.UserType,
                    SentAt = p.CreatedAt,
                    Success = p.Success,
                    Error = p.Error
                })
                .ToListAsync();

            return notiList;
        }

        public List<Notification> GetListByUser(int userId)
        {
            List<Notification> notiList = new List<Notification>();
            notiList = _db.Notifications.Where(p => p.UserId == userId).ToList();
            // mark all notifications as read when customer opens notification list page
            List<Notification> unread = _db.Notifications.Where(p => p.Status == NotificationStatus.UNREAD).ToList();
            foreach (var noti in unread)
            {
                noti.Status = NotificationStatus.READ;
                _db.Entry(noti).State = EntityState.Modified;
                _db.SaveChanges();
            }
            return notiList;
        }

        public int GetTotalUnreadNotifications(int userId)
        {
            int count = _db.Notifications.Where(p => p.UserId == userId && p.Status == NotificationStatus.UNREAD).Count();
            return count;
        }

        public async Task<bool> UpdateProfileOrderDetails(List<RecentCustomerOrder> orders)
        {
            var profiles = await _db.Profiles.Where(p => orders.Select(p => p.UserId).ToList().Contains(p.UserId)).ToListAsync();
            int count = 0;
            foreach (var order in orders)
            {
                var profile = profiles.Where(p => p.UserId == order.UserId).FirstOrDefault();
                if (profile != null)
                {
                    profile.LastOrderedAt = order.LastOrderedAt;
                    profile.LastOrderDeliveredAt = order.LastOrderDeliveredAt;
                    _db.Entry(profile).State = EntityState.Modified;
                }

                count++;
            }

            if (count > 0)
            {
                await _db.SaveChangesAsync();
            }
            
            return count > 0;
        }

        public FirebasePushNotificationResponse FirebasePushNotification(Notification noti, Source app)
        {
            try
            {
                //return true;
                string applicationID = "";
                string senderId = "";
                if (app == Source.CUSTOMER_APP)
                {
                    applicationID = _apiSettings.Value.CustomerFirebaseServerKey;
                    senderId = _apiSettings.Value.CustomerFirebaseSenderID;
                }
                else if (app == Source.DRIVER_APP)
                {
                    applicationID = _apiSettings.Value.DriverFirebaseServerKey;
                    senderId = _apiSettings.Value.DriverFirebaseSenderID;
                }
                else if (app == Source.RELAYPOINT_APP)
                {
                    applicationID = _apiSettings.Value.CustomerFirebaseServerKey;
                    senderId = _apiSettings.Value.CustomerFirebaseSenderID;
                }
                string deviceId = noti.DeviceId;

                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");

                tRequest.Method = "post";

                tRequest.ContentType = "application/json";

                var data = new object();
                if (noti.PushNotificationType == PushNotificationType.NOTIFICATION)
                {
                    data = new
                    {
                        to = deviceId,
                        notification = new
                        {
                            title = noti.Title ?? "EasyGas Notification",
                            body = noti.Description,
                            android_channel_id = "high_importance_channel",
                            icon = noti.Type.ToString(),
                            image = noti.ImageUrl,
                        }
                    };
                }
                else
                {
                    data = new
                    {
                        to = deviceId,
                        notification = new
                        {
                            title = noti.Title ?? "EasyGas Notification",
                            body = noti.Description,
                            android_channel_id = "high_importance_channel",
                            icon = noti.Type.ToString(),
                            image = noti.ImageUrl,
                        },
                        data = new
                        {
                            title = noti.Title ?? "EasyGas Notification",
                            body = noti.Description,
                            icon = noti.Type.ToString(),
                            category = noti.Category,
                            imageUrl = noti.ImageUrl,
                            webUrl = noti.GoToWebUrl
                        }
                    };
                }

                //var serializer = new System.Web.Extensions.JavaScriptSerializer();


                //object o = JsonConvert.DeserializeObject(data);
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                //var json = serializer.Serialize(data);

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));

                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));

                tRequest.ContentLength = byteArray.Length;


                using (Stream dataStream = tRequest.GetRequestStream())
                {

                    dataStream.Write(byteArray, 0, byteArray.Length);


                    using (WebResponse tResponse = tRequest.GetResponse())
                    {

                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {

                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {

                                String sResponseFromServer = tReader.ReadToEnd();
                                _logger.LogInformation("NotificationMgr.FirebasePushNotification {response}", sResponseFromServer);

                                var response = JsonConvert.DeserializeObject<FirebasePushNotificationResponse>(sResponseFromServer);
                                return response;
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogCritical("NotificationMgr.FirebasePushNotification {noti} {@ex}", noti, ex);
            }


            //try
            //{
            //    noti.UserId = 0;
            //    var notification = new NotificationModel
            //    {
            //        DeviceId = noti.DeviceId,
            //        Description = noti.Description,
            //        Type = noti.Type.ToString(),
            //        Timestamp = DateMgr.GetCurrentIndiaTime().ToString("dd/MM/yyyy HH:mm")
            //    };
            //    var firebaseClient = new FirebaseClient(_apiSettings.Value.FirebaseUrl);
            //    if (noti.UserType == UserType.DRIVER)
            //    {
            //        var result = await firebaseClient
            //      .Child("Drivers/" + noti.DeviceId + "/Notifications")
            //      .PostAsync(notification);
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            return null;
        }

        public async Task<CommandResult> AddCustomerNotificationTemplate(AddNotificationTemplateRequest request)
        {
            List<string> validationErrors = new List<string>();
            if (string.IsNullOrEmpty(request.NotificationName))
            {
                validationErrors.Add("Name is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            if(string.IsNullOrEmpty(request.Title))
            {
                validationErrors.Add("Title is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                validationErrors.Add("Description is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            var existingTemplate = _db.CustomerNotificationTemplates.Where(p => p.NotificationName == request.NotificationName).Any();
            if (existingTemplate)
            {
                validationErrors.Add("Template with same name already exists.");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            string imageUrl = null;
            if(!string.IsNullOrEmpty(request.ImageBase64String) && !string.IsNullOrEmpty(request.ImageExtension))
            {
                var imageData = GetImageData(request.ImageBase64String);
                if (imageData != null)
                {
                    string fileName = "CustomerNoti" + Guid.NewGuid().ToString() + request.ImageExtension;
                    var isSaved = await SaveDataToBlob(imageData, fileName, _apiSettings.Value.BlobCustomerNotificationsContainer);
                    if (isSaved)
                    {
                        string storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerNotificationsContainer;
                        imageUrl = storageUrl + "/" + fileName;
                    }
                }
            }
            NotificationTemplate notificationTemplate = new NotificationTemplate(
                request.NotificationName, 
                request.Title, 
                request.Description, 
                imageUrl, 
                request.CouponCode, 
                request.IsActive, 
                request.Channel);
            try
            {
                _db.CustomerNotificationTemplates.Add(notificationTemplate);
                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return new CommandResult(ex, HttpStatusCode.InternalServerError);
            }
            return new CommandResult(System.Net.HttpStatusCode.OK);
        }

        public async Task<CommandResult> UpdateCustomerNotificationTemplate(UpdateNotificationTemplateRequest request)
        {
            List<string> validationErrors = new List<string>();
            if (string.IsNullOrEmpty(request.NotificationName))
            {
                validationErrors.Add("Name is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            if (string.IsNullOrEmpty(request.Title))
            {
                validationErrors.Add("Title is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                validationErrors.Add("Description is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            var template = _db.CustomerNotificationTemplates.Where(p => p.Id == request.Id).FirstOrDefault();
            if(template == null)
            {
                validationErrors.Add("Not found");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
            var existingName = _db.CustomerNotificationTemplates.Where(p => p.NotificationName == request.NotificationName && p.Id != request.Id).Any();
            if (existingName)
            {
                validationErrors.Add("Template with same name already exists");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            string imageUrl = null;
            if (!string.IsNullOrEmpty(request.ImageBase64String) && !string.IsNullOrEmpty(request.ImageExtension))
            {
                var imageData = GetImageData(request.ImageBase64String);
                if (imageData != null)
                {
                    string fileName = "CustomerNoti" + Guid.NewGuid().ToString() + request.ImageExtension;
                    var isSaved = await SaveDataToBlob(imageData, fileName, _apiSettings.Value.BlobCustomerNotificationsContainer);
                    if (isSaved)
                    {
                        string storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerNotificationsContainer;
                        imageUrl = storageUrl + "/" + fileName;
                    }
                }
            }
            template.Update(request.NotificationName,request.Title,request.Description,imageUrl,request.CouponCode,request.IsActive,request.Channel);
            try
            {
                _db.Entry(template).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return new CommandResult(ex, HttpStatusCode.InternalServerError);
            }
            return new CommandResult(HttpStatusCode.OK,template);


        }

        public async Task<List<NotificationTemplate>> GetActiveCustomerNotificationTemplates()
        {
            var templates = await _db.CustomerNotificationTemplates.Where(p => p.IsActive == true).ToListAsync();
            return templates;
        }

        public async Task<CommandResult> GetCustomerNotificationTemplate(int id)
        {
            List<string> validationErrors= new List<string>();
            NotificationTemplate template = await _db.CustomerNotificationTemplates.Where(p => p.Id == id).FirstOrDefaultAsync();
            if(template == null)
            {
                validationErrors.Add("Not found");
                return new CommandResult(HttpStatusCode.OK, validationErrors);
            }
            return new CommandResult(HttpStatusCode.OK,template);
        }
    }
}

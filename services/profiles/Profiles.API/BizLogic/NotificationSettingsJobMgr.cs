using EasyGas.Shared;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Profiles.API.ViewModels.Notification;
using Newtonsoft.Json;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class NotificationSettingsJobMgr
    {
        private readonly ProfilesDbContext _db;
        private readonly ILogger _logger;
        private readonly IOptions<ApiSettings> _apiSettings;
        private NotificationMgr _notiMgr;

        private List<User> _customers; 

        //private List<Order> _orders;
        private List<Notification> _notifications;
        private List<string> _installedDevicesList;

        public NotificationSettingsJobMgr(ProfilesDbContext db, NotificationMgr notiMgr, IOptions<ApiSettings> apiSettings, ILogger<NotificationSettingsJobMgr> logger)
        {
            _db = db;
            _notiMgr = notiMgr;
            _apiSettings = apiSettings;
            _logger = logger;
        }

        public async Task<CommandResult> SendToInstalledCustomersNotRegistered(NotificationSettings settings)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                GetAllCustomers();
                GetAllInstalledDevices();

                if (_installedDevicesList.Count == 0)
                {
                    validationErrors.Add("No installed devices found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var now = DateMgr.GetCurrentIndiaTime();

                foreach (var deviceId in _installedDevicesList)
                {
                    if (_customers.Any(p => p.Profile.DeviceId == deviceId))
                    {
                        continue;
                    }

                    Notification sendNoti = await SendNotification(settings, now, null, deviceId);
                    if (sendNoti != null)
                    {
                        _db.Notifications.Add(sendNoti);
                        count += 1;
                    }

                    if (count > 0 && count % 100 == 0)
                    {
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("NotificationSettingsJobMgr.SendToInstalledCustomersNotRegistered Successfully saved " + count + " notifications ");
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("NotificationSettingsJobMgr.SendToInstalledCustomersNotRegistered Success | Notification successfully sent to " + count + " customers ");

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to " + count + " customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification successfully sent to " + count + " customers with exception");
                _logger.LogCritical("NotificationSettingsJobMgr.SendToInstalledCustomersNotRegistered Exception | Notification successfully sent to " + count + " customers with exeption | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SendToRegisteredCustomersNotOrdered(NotificationSettings settings)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                GetAllCustomers();

                if (_customers.Count == 0)
                {
                    validationErrors.Add("No customers found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var now = DateMgr.GetCurrentIndiaTime();

                foreach (var user in _customers)
                {
                    
                    if (user.Profile.LastOrderedAt != null)
                    {
                        continue;
                    }
                    
                    if (!CanSendToCustomer(user, settings, now))
                    {
                        continue;
                    }

                    Notification sendNoti = await SendNotification(settings, now, user);
                    if (sendNoti != null)
                    {
                        _db.Notifications.Add(sendNoti);
                        count += 1;
                    }

                    if (count > 0 && count % 100 == 0)
                    {
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("NotificationSettingsJobMgr.SendToRegisteredCustomersNotOrdered Successfully saved " + count + " notifications ");
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("NotificationSettingsJobMgr.SendToRegisteredCustomersNotOrdered Success | Notification successfully sent to " + count + " customers ");

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to " + count + " customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification successfully sent to " + count + " customers with exception");
                _logger.LogCritical("NotificationSettingsJobMgr.SendToRegisteredCustomersNotOrdered Exception | Notification successfully sent to " + count + " customers with exeption | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SendToRegisteredCustomers(NotificationSettings settings)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                GetAllCustomers();

                if (_customers.Count == 0)
                {
                    validationErrors.Add("No customers found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var now = DateMgr.GetCurrentIndiaTime();

                foreach (var user in _customers)
                {
                    if (!CanSendToCustomer(user, settings, now))
                    {
                        continue;
                    }

                    Notification sendNoti = await SendNotification(settings, now, user);
                    if (sendNoti != null)
                    {
                        _db.Notifications.Add(sendNoti);
                        count += 1;
                    }

                    if (count > 0 && count % 100 == 0)
                    {
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("NotificationSettingsJobMgr.SendToRegisteredCustomers Successfully saved " + count + " notifications ");
                    }
                }
                await _db.SaveChangesAsync();
                _logger.LogInformation("NotificationSettingsJobMgr.SendToRegisteredCustomers Success | Notification successfully sent to " + count + " customers ");

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to " + count + " customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification successfully sent to " + count + " customers with exception");
                _logger.LogCritical("NotificationSettingsJobMgr.SendToRegisteredCustomers Exception | Notification successfully sent to " + count + " customers with exeption | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SendToOrderedCustomers(NotificationSettings settings)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                GetAllCustomers();
                if (_customers.Count == 0)
                {
                    validationErrors.Add("No customers found");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var now = DateMgr.GetCurrentIndiaTime();

                foreach (var user in _customers)
                {
                    
                    if (user.Profile.LastOrderedAt == null)
                    {
                        continue;
                    }
                    
                    if (!CanSendToCustomer(user, settings, now))
                    {
                        continue;
                    }

                    Notification sendNoti = await SendNotification(settings, now, user);
                    if (sendNoti != null)
                    {
                        _db.Notifications.Add(sendNoti);
                        count += 1;
                    }

                    if (count > 0 && count % 100 == 0)
                    {
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("NotificationSettingsJobMgr.SendToOrderedCustomers Successfully saved " + count + " notifications ");
                    }
                }
                await _db.SaveChangesAsync();
                _logger.LogInformation("NotificationSettingsJobMgr.SendToOrderedCustomers Success | Notification successfully sent to " + count + " customers ");

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to " + count + " customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification successfully sent to " + count + " customers with exception");
                _logger.LogCritical("NotificationSettingsJobMgr.SendToOrderedCustomers Exception | Notification successfully sent to " + count + " customers with exeption | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        private bool CanSendToCustomer(User user, NotificationSettings notiSettings, DateTime today)
        {
            try
            {
                if (notiSettings.NotiTriggerType == NotificationTriggerType.AUTO_SEND_FREQUENCY && notiSettings.Frequency > 0)
                {
                    GetAllNotifications();
                    double? daysPast = null;
                    Notification lastSend = null;
                    if (_notifications != null)
                    {
                        lastSend = _notifications.Where(p => p.UserId == user.Id && p.UserCategory == notiSettings.NotiUserCategory).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                    }
                    if (lastSend == null)
                    {
                        if (notiSettings.NotiUserCategory == NotificationUserCategory.CUSTOMER_REGISTERED || notiSettings.NotiUserCategory == NotificationUserCategory.CUSTOMER_REGISTERED_NOT_ORDERED)
                        {
                            daysPast = (today.Date - user.CreatedAt.Date).TotalDays;
                        }
                        else if (notiSettings.NotiUserCategory == NotificationUserCategory.CUSTOMER_ORDERED)
                        {
                            DateTime? lastOrderedAt = user.Profile.LastOrderedAt;
                            if (lastOrderedAt != null)
                            {
                                daysPast = (today.Date - lastOrderedAt.Value.Date).TotalDays;
                            }

                            /*
                            Order lastOrdered = null;
                            if (_orders != null)
                            {
                                lastOrdered = _orders.Where(p => p.UserId == user.Id && (p.Status == OrderStatus.DELIVERED || p.Status == OrderStatus.DISPATCHED || p.Status == OrderStatus.VEHICLE_ASSIGNED || p.Status == OrderStatus.APPROVED)).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                            }
                            if (lastOrdered != null)
                            {
                                daysPast = (today.Date - lastOrdered.CreatedAt.Date).TotalDays;
                            }
                            */
                        }
                    }
                    else
                    {
                        daysPast = (today.Date - lastSend.CreatedAt.Date).TotalDays;
                        
                    }

                    if (daysPast != null && daysPast >= notiSettings.Frequency)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                _logger.LogCritical("NotificationSettingsJobMgr.CanSendToCustomer Exception | " + ex.ToString());
            }
            return false;
        }

        public async Task<CommandResult> RunCronJob(int branchId)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                _logger.LogInformation("NotificationsSettingsJobMgr.RunCronJob Started");
                var settings = _db.NotificationSettings.Where(p => p.IsActive == true && p.BranchId == branchId).ToList();
                foreach(var setting in settings)
                {
                    if (setting.NotiTriggerType == NotificationTriggerType.AUTO_SEND_SCHEDULED)
                    {
                        await SendScheduledNotifications(setting);
                    }
                    else if (setting.NotiTriggerType == NotificationTriggerType.AUTO_SEND_FREQUENCY)
                    {
                        await SendFrequencyNotifications(setting);
                    }
                }

                _logger.LogInformation("NotificationsSettingsJobMgr.RunCronJob Completed");
                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Cron Job completed"
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("RunCronJob with exception: " + ex.Message);
                _logger.LogCritical("NotificationSettingsJobMgr.RunCronJob Exception | " + ex.ToString());
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SendScheduledNotifications(NotificationSettings setting)
        {
            List<string> validationErrors = new List<string>();
            bool runNow = false;
            try
            {
                DateTime now = DateMgr.GetCurrentIndiaTime();

                    // if scheduled date is today
                    if (setting.ScheduledDate.HasValue && setting.ScheduledDate.Value.Date == now.Date)
                    {
                        // if scheduled time is fixed
                        if (setting.NotiTriggerTime == NotificationTriggerTime.FIXED)
                        {
                            if (setting.FromTime.HasValue)
                            {
                                DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, setting.FromTime.Value.Hour, setting.FromTime.Value.Minute, 0);
                                // if scheduled time is not in future
                                if (scheduledTime <= now && (scheduledTime > setting.LastCronJobRunAt || setting.LastCronJobRunAt == null))
                                {
                                    runNow = true;
                                }
                            }
                        }
                        else
                        {

                        // if it is already run for today, dont run again
                        if (setting.LastCronJobRunAt != null && setting.LastCronJobRunAt >= now.Date)
                        {
                            _logger.LogInformation("NotificationsSettingsJobMgr.RunCronJob Cron job already run for today");
                            validationErrors.Add("Cron job already run for today ");
                            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                        }

                        int randomStartTimeSec = 25200; // 7AM
                            int randomEndTimeSec = 57600; // 4PM
                            if (now.Date.AddSeconds(randomStartTimeSec) <= now)
                            {
                                if (now.Date.AddSeconds(randomEndTimeSec) <= now) // if end time is over and cron job is not run, run it now
                                {
                                    runNow = true;
                                }
                                else // send it randomly
                                {
                                    var random = new Random();
                                    if ((random.Next(1000) % 3) == 0) // just to make it random
                                    {
                                        runNow = true;
                                    }
                                }
                            }
                        }
                    }

                    if (runNow)
                    {
                        setting.LastCronJobRunAt = now;
                        _db.Entry(setting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await SendNotificationsByUserCategory(setting);
                    }

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification with exception");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SendFrequencyNotifications(NotificationSettings setting)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            bool runNow = false;
            try
            {
                DateTime now = DateMgr.GetCurrentIndiaTime();

                // if it is already run for today, dont run again
                if (setting.LastCronJobRunAt != null && setting.LastCronJobRunAt >= now.Date)
                {
                    validationErrors.Add("Cron job already run for today ");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                    // if scheduled time is fixed
                    if (setting.NotiTriggerTime == NotificationTriggerTime.FIXED)
                    {
                        if (setting.FromTime.HasValue)
                        {
                            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, setting.FromTime.Value.Hour, setting.FromTime.Value.Minute, 0);
                            // if scheduled time is not in future
                            if (scheduledTime <= now)
                            {
                                runNow = true;
                            }
                        }
                    }
                    else
                    {
                    int randomStartTimeSec = 25200; // 7AM
                    int randomEndTimeSec = 57600; // 4PM
                    if (now.Date.AddSeconds(randomStartTimeSec) <= now)
                        {
                            if (now.Date.AddSeconds(randomEndTimeSec) <= now) // if end time is over and cron job is not run, run it now
                            {
                                runNow = true;
                            }
                            else // send it randomly
                            {
                                var random = new Random();
                                if ((random.Next(1000) % 3) == 0) // just to make it random
                                {
                                    runNow = true;
                                }
                            }
                        }
                    }

                    if (runNow)
                    {
                        setting.LastCronJobRunAt = now;
                        _db.Entry(setting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await SendNotificationsByUserCategory(setting);
                        _db.Entry(setting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }

                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to " + count + " customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification exception");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<Notification> SendNotification(NotificationSettings settings, DateTime now, User user = null, string deviceId = "")
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId) && user != null)
                {
                    deviceId = user.Profile.DeviceId;
                }
                Notification newNoti = new Notification()
                {
                    UserType = UserType.CUSTOMER,
                    Title = settings.Title,
                    Description = settings.Description,
                    ImageUrl = settings.IsImageActive == true ? settings.Imageurl : "",
                    GoToWebUrl = settings.GoToWebUrl,
                    Type = NotificationType.INFO,
                    Category = NotificationCategory.CUSTOMER_PROMOTIONS,
                    PushNotificationType = PushNotificationType.DATA,
                    UserId = user?.Id,
                    DeviceId = deviceId,
                    Status = NotificationStatus.UNREAD,
                    CreatedBy = "",
                    UpdatedBy = "",
                    CreatedAt = now,
                    UpdatedAt = now,
                    TriggerTime = settings.NotiTriggerTime,
                    TriggerType = settings.NotiTriggerType,
                    UserCategory = settings.NotiUserCategory
                };


                /*
                if (user.UserName != "9400407370" ) //TODO remove after testing
                {
                    return null;
                }
                */

                if (!string.IsNullOrEmpty(newNoti.DeviceId))
                {
                    var notiResp = _notiMgr.FirebasePushNotification(newNoti, Source.CUSTOMER_APP);
                    if (notiResp != null)
                    {
                        newNoti.FirebaseResponse = JsonConvert.SerializeObject(notiResp);
                        newNoti.Success = notiResp.success == 1;
                        if (!newNoti.Success)
                            newNoti.Error = notiResp.results?[0].error;

                        return newNoti;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<CommandResult> SendNotificationsByUserCategory(NotificationSettings settings)
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                switch (settings.NotiUserCategory)
                {
                    case NotificationUserCategory.CUSTOMER_REGISTERED:
                        await SendToRegisteredCustomers(settings);
                        break;
                    case NotificationUserCategory.CUSTOMER_REGISTERED_NOT_ORDERED:
                        await SendToRegisteredCustomersNotOrdered(settings);
                        break;
                    case NotificationUserCategory.CUSTOMER_ORDERED:
                        await SendToOrderedCustomers(settings);
                        break;
                    case NotificationUserCategory.CUSTOMER_INSTALLED_NOT_REGISTERED:
                        await SendToInstalledCustomersNotRegistered(settings);
                        break;
                }
                return new CommandResult(System.Net.HttpStatusCode.OK, new
                {
                    message = "Notification successfully sent to customers.",
                });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Notification successfully sent to " + count + " customers with exception");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        private void GetAllNotifications()
        {
            if (_notifications == null)
            {
                var fromDate = DateMgr.GetCurrentIndiaTime().Date.AddDays(-60); //last 60 days
                _notifications = _db.Notifications.Where(p => p.CreatedAt >= fromDate).ToList();
            }
        }

        private void GetAllCustomers()
        {
            if (_customers == null)
            {
                _customers = _db.Users
                    .Include(p => p.Profile)
                    .Where(p => p.Type == UserType.CUSTOMER && p.Profile.DeviceId != "" && p.Profile.DeviceId != null).ToList();
            }
        }

        /*
        private void GetAllOrders()
        {
            if (_orders == null)
            {
                var fromDate = DateMgr.GetCurrentIndiaTime().Date.AddDays(-60); //last 60 days
                _orders = _db.Orders.Where(p => p.CreatedAt >= fromDate && (p.Status == OrderStatus.DELIVERED || p.Status == OrderStatus.DISPATCHED || p.Status == OrderStatus.APPROVED || p.Status == OrderStatus.VEHICLE_ASSIGNED)).ToList();
            }
            
        }
        */

        private void GetAllInstalledDevices()
        {
            if (_installedDevicesList == null)
            {
                _installedDevicesList = _db.UserDevices.Where(p => p.UserId == null).Select(p => p.FirebaseDeviceId).Distinct().ToList();
            }
        }
    }
}

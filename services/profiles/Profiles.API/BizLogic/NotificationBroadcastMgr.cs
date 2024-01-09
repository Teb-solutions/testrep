using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using EasyGas.Services.Profiles.Data;
using Profiles.API.ViewModels.Broadcast;
using EasyGas.Shared.Formatters;
using EasyGas.Shared.Enums;
using EasyGas.BuildingBlocks.EventBus.Events;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class NotificationBroadcastMgr
    {
        
        private readonly ProfilesDbContext _db;
        private readonly ILogger _logger;
        private readonly IOptions<ApiSettings> _apiSettings;

        public NotificationBroadcastMgr(ProfilesDbContext db, IOptions<ApiSettings> apiSettings, ILogger<NotificationBroadcastMgr> logger)
        {
            _db = db;
            _apiSettings = apiSettings;
            _logger = logger;
        }

        public async Task<bool> BroadcastToDrivers(UpdateExpressBroadcastResponse req, string itemSummary)
        {
            try
            {
                var currentTime = DateMgr.GetCurrentIndiaTime();
                foreach (var broadcastVeh in req.Vehicles)
                {
                    NotificationBroadcastModel newBroadcast = new NotificationBroadcastModel()
                    {
                        UserId = (int)broadcastVeh.DriverId,
                        DeviceId = broadcastVeh.DriverDeviceId,
                        BookingId = req.OrderId,
                        BookingCode = req.OrderCode,
                        BookingType = itemSummary,
                        CustAddress = req.OrderAddress,
                        TimeOutSec = req.TimeOutSec,
                        TimeOfDelivery = req.DeliveryTo.ToString("dd/MM/yyyy HH:mm"),
                        CylinderQuantity = itemSummary,
                        Description = "Express Order Request",
                        BroadcastType = BroadcastType.BROADCAST
                    };

                    var firebaseNoti = FirebasePushNotification(newBroadcast);
                }
                return await Task.FromResult(true);

            }
            catch (Exception ex)
            {
                _logger.LogCritical("NotificationBroadcastMgr.BroadcastToDrivers Exception - " + ex.ToString());
                return false;
            }
        }


        
        public async Task<bool> BroadcastStopToDriverWhenOrderAccepted(int orderId, string orderCode, List<ExpressOrderConfirmedEventDriverDetail> BroadcastStopDriverList)
        {
            try
            {
                foreach (var broadcastUser in BroadcastStopDriverList)
                {
                    NotificationBroadcastModel newStopBroadcast = new NotificationBroadcastModel()
                    {
                        UserId = broadcastUser.DriverId,
                        DeviceId = broadcastUser.DriverDeviceId,
                        BookingId = orderId,
                        BookingCode = orderCode,
                        Description = "Express Order Stop Request",
                        BroadcastType = BroadcastType.STOP
                    };

                    var firebaseNoti = FirebasePushNotification(newStopBroadcast);
                }

                return await Task.FromResult(true);

            }
            catch (Exception e)
            {
                _logger.LogCritical("BroadcastStopToDriverWhenOrderAccepted Exception - " + e.ToString());
                return false;
            }
        }

        /*

        public async Task<CommandResult> RejectOrder(BroadcastAcceptModel model)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var now = DateMgr.GetCurrentIndiaTime();
                Order order = _db.Orders.Include(u => u.User).Include(u => u.User.Profile).Include(u => u.Address).Where(p => p.Id == model.OrderId).FirstOrDefault();
                if (order == null)
                {
                    validationErrors.Add("Order is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (order.Status != OrderStatus.APPROVED && order.Status != OrderStatus.CREATED)
                {
                    validationErrors.Add("Order is not active.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                var broadcastDriver = _db.BroadcastDrivers.Include(p => p.Vehicle).Where(p => p.OrderId == model.OrderId && p.DriverId == model.DriverId && p.VehicleId == model.VehicleId).FirstOrDefault();
                if (broadcastDriver == null)
                {
                    validationErrors.Add("Driver is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                DateTime timeOut = broadcastDriver.TimeOut;
                var settings = _db.Settings.Where(p => p.TenantId == order.TenantId).FirstOrDefault();
                if (settings != null)
                {
                    timeOut = timeOut.AddSeconds(settings.BroadcastTimeOutBackendBufferSec);
                }
                if (now >= timeOut)
                {
                    validationErrors.Add("Broadcast is timed out.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                broadcastDriver.Type = BroadcastType.REJECT;
                broadcastDriver.DriverActionAt = now;
                //broadcastDriver.IsActive = false;
                _db.Entry(broadcastDriver).State = EntityState.Modified;

                await _db.SaveChangesAsync();
                _logger.LogInformation("NotificationBroadcastMgr.RejectOrder | Success | orderId: " + model.OrderId + ", driverId: " + model.DriverId);

                return new CommandResult(System.Net.HttpStatusCode.OK, new ExpressOrderResponse
                {
                    OrderId = order.Id,
                    OrderCode = order.Code,
                    OrderStatus = order.Status
                });
            }
            catch (Exception e)
            {
                _logger.LogCritical("NotiBroadcastMgr RejectOrder Exception - " + e.ToString());
                validationErrors.Add(e.ToString());
                //validationErrors.Add("Some internal error has occured.");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }
        */

        public bool FirebasePushNotification(NotificationBroadcastModel noti)
        {
            try
            {
                var applicationID = _apiSettings.Value.DriverFirebaseServerKey;
                var senderId = _apiSettings.Value.DriverFirebaseSenderID;
                string deviceId = noti.DeviceId;
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                var data = new
                {
                    to = deviceId,
                    data = new
                     {
                         body = noti.Description,
                         bookingId = noti.BookingId,
                         bookingCode = noti.BookingCode,
                         bookingType = noti.BookingType,
                         customerName = noti.CustName,
                         customerMobile = noti.CustMobile,
                         customerAddress = noti.CustAddress,
                         deliveryTime = noti.TimeOfDelivery,
                         timeOutSec = noti.TimeOutSec, 
                         quantity = noti.CylinderQuantity,
                         //type = noti.BroadcastType, // can be removed, used only in old versions of driver app
                         category = noti.BroadcastType == BroadcastType.BROADCAST ? NotificationCategory.DRIVER_BROADCAST_START : NotificationCategory.DRIVER_BROADCAST_STOP,
                         title = "EasyGas Express Delivery",
                         icon = "Alert"
                     } 
                };
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
                                string str = sResponseFromServer;
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                string str = ex.Message;
                return false;
            }
            return true;
        }
        
    }
}

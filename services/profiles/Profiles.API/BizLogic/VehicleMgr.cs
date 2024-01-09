using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using Microsoft.Extensions.Logging;
using EasyGas.Shared.Models;
using Profiles.API.ViewModels.Report;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class VehicleMgr
    {
        private ProfilesDbContext _db;
        private NotificationMgr _notiMgr;
        private LocationService _vehService;
        private readonly ILogger<VehicleMgr> _logger;

        public VehicleMgr(ProfilesDbContext db, NotificationMgr notiMgr, ILogger<VehicleMgr> logger, LocationService vehService)
        {
            _db = db;
            _notiMgr = notiMgr;
            _vehService = vehService;
            _logger = logger;
        }

        public async Task<CommandResult> DriverOnStateChange(int driverUserId, DLoginState dls, DActivityState das, double? lat, double? lng)
        {
            try
            {
                DateTime now = DateMgr.GetCurrentIndiaTime();

                Vehicle vehicle = _db.Vehicles.Where(p => p.DriverId == driverUserId && p.IsActive).FirstOrDefault();
                if (vehicle == null)
                {
                    return CommandResult.FromValidationErrors("Driver not assigned to any vehicle");
                }

                if (lat != null && lng != null)
                {
                    if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
                    {
                        _logger.LogCritical($"VehicleMgr.DriverOnStateChange Location invalid | driverId: {driverUserId} | lat:{lat} lng:{lng}");
                        return CommandResult.FromValidationErrors("Location is invalid");
                    }
                }

                switch (dls)
                {
                    case DLoginState.LoggedIN:
                        vehicle.State = VehicleState.ReadyForWork;
                        break;
                    case DLoginState.PauseJob:
                        vehicle.State = VehicleState.Break;
                        break;
                    case DLoginState.ResumeJob:
                        vehicle.State = VehicleState.ReadyForWork;
                        break;
                    case DLoginState.LoggedOut:
                        vehicle.State = VehicleState.OutFromWork;
                        break;
                }

                _db.Entry(vehicle).State = EntityState.Modified;

                DriverActivity lastAcivity = _db.DriverActivities.Where(p => p.UserId == driverUserId && p.VehicleId == vehicle.Id).OrderByDescending(p => p.DutyStart).FirstOrDefault();
                if (lastAcivity != null)
                {
                    if (lastAcivity.DutyEnd == null)
                    {
                        lastAcivity.DutyEnd = now;
                        _db.Entry(lastAcivity).State = EntityState.Modified;
                    }
                }


                //if (state == VehicleState.ReadyForWork)
                //{
                DriverActivity newActivity = new DriverActivity
                {
                    UserId = driverUserId,
                    VehicleId = vehicle.Id,
                    DutyStart = now,
                };

                if (lat != null && lng != null)
                {
                    newActivity.CurrentLat = (double)lat;
                    newActivity.CurrentLng = (double)lng;
                }
                newActivity.CurrentTime = now;
                newActivity.das = das;
                newActivity.dls = dls;
                
                //  _db.DriverActivities.Add(newActivity);

                //}
                
                _db.DriverActivities.Add(newActivity);
                await _db.SaveChangesAsync();

                return new CommandResult(System.Net.HttpStatusCode.OK, vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"VehicleMgr.DriverOnStateChange Catch | driverId: {driverUserId} | {ex.ToString()}");
                return CommandResult.FromValidationErrors("Some internal error occured.");
            }
        }

        public async Task<CommandResult> GetActivityStatus(int driverUserId)
        {
            try
            {
                DateTime now = DateMgr.GetCurrentIndiaTime();

                Vehicle vehicle = await _db.Vehicles.Where(p => p.DriverId == driverUserId).FirstOrDefaultAsync();
                if (vehicle == null)
                {
                    return CommandResult.FromValidationErrors("Driver not assigned to any vehicle");
                }

                DriverActivityResponse response = new DriverActivityResponse()
                {
                    VehicleState = vehicle.State
                };

                DriverActivity lastAcivity = _db.DriverActivities.Where(p => p.UserId == driverUserId && p.VehicleId == vehicle.Id).OrderByDescending(p => p.DutyStart).FirstOrDefault();

                if (lastAcivity != null)
                {
                    response.DriverActivityState = lastAcivity.das;
                    response.DriverLoginState = lastAcivity.dls;
                }

                return new CommandResult(System.Net.HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"VehicleMgr.GetActivityStatus Catch | driverId: {driverUserId} | {ex.ToString()}");
                return CommandResult.FromValidationErrors("Some internal error occured.");
            }
        }
        public double DistanceBetweenPlacesinMtr(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));
            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;
            double d = Math.Acos(cosD);
            double dist = R * d * 1000;
            //dist = 1;
            dist += dist * 30 / 100; //it is road distance so I am adding 30%

            /*
              var R = 6371; // Radius of the earth in km
                var dLat = deg2rad(lat2 - lat1);  // deg2rad below
                var dLon = deg2rad(lng2 - lng1);
                var a =
                  Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                  Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                  Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                  ;
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = R * c; // Distance in km
                */
            return dist > 0 ? dist : 0;
        }

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        private double GetVehicleEta(double lat1, double lon1, double lat2, double lon2, double vehSpeed)
        {
            double EtaInMin = 1;
            try
            {
                double distance = DistanceBetweenPlacesinMtr(lat1, lon1, lat2, lon2);
                double etaInSec = distance / vehSpeed;
                if (etaInSec > 0)
                {
                    EtaInMin = Math.Round(etaInSec / 60);
                }
                EtaInMin = EtaInMin == 0 ? 1 : EtaInMin;
            }
            catch(Exception ex)
            {

            }
            return EtaInMin;
        }


        /*
        public async Task<VehicleLocationsViewModel> GetVehicleTrackingDetails(string orderCode)
        {
            VehicleLocationsViewModel vehTrackDetails = new VehicleLocationsViewModel();
            double vehSpeed = 10; // in m/s
            DateTime now = DateMgr.GetCurrentIndiaTime();
            try
            {
                var order = _db.Orders.Include(p => p.Address).Where(p => p.Code == orderCode).FirstOrDefault();
                if (order != null)
                {
                    if (order.Status == OrderStatus.DISPATCHED && order.VehicleId != null)
                    {
                        vehTrackDetails = await _vehService.GetVehicleLocation((int)order.VehicleId);
                        if (vehTrackDetails.Id > 0)
                        {
                            vehTrackDetails.Eta = (int)GetVehicleEta(order.Address.Lat, order.Address.Lng, vehTrackDetails.Lat, vehTrackDetails.Lng, vehSpeed);
                            vehTrackDetails.EstimatedArrivalTime = now.AddMinutes((int)vehTrackDetails.Eta);
                            if (vehTrackDetails.EstimatedArrivalTime > order.DeliveryTo)
                            {
                                vehTrackDetails.ArrivalStatus = "Late";
                            }
                            else
                            {
                                vehTrackDetails.ArrivalStatus = "In Time";
                            }
                        }
                    }
                    else
                    {
                        if (order.Status  < OrderStatus.DISPATCHED)
                        {
                            if (now > order.DeliveryTo)
                            {
                                vehTrackDetails.ArrivalStatus = "Late";
                            }
                            else
                            {
                                vehTrackDetails.ArrivalStatus = "In Time";
                            }
                        }
                        else
                        {
                            if (order.DeliveredAt > order.DeliveryTo)
                            {
                                vehTrackDetails.ArrivalStatus = "Late";
                            }
                            else
                            {
                                vehTrackDetails.ArrivalStatus = "In Time";
                            }
                        }
                    }
                    vehTrackDetails.OrderStatus = order.Status;
                }
            }
            catch(Exception ex)
            {

            }
            return vehTrackDetails;
        }

        public long GetVehicleTravelledMetersByFormula(List<Order> orderList, int? vehId, int? driverId, DateTime fromDate, DateTime toDate)
        {
            long totDistance = 0;
            try
            {
                /*-----based on dispatched time
            List<Order> dispatchedOrders = _db.Orders.Where(p => p.VehicleId == vehId && p.DispatchedAt >= fromDate && p.DispatchedAt <= toDate).ToList();
            foreach (var order in dispatchedOrders)
            {
                if (order.TripOriginLat != null && order.TripOriginLng != null && order.TripDestinationLat != null && order.TripDestinationLng != null)
                {
                    double distance = DistanceBetweenPlacesinMtr((double)order.TripOriginLat, (double)order.TripOriginLng, (double)order.TripDestinationLat, (double)order.TripDestinationLng);
                    totDistance += (long)distance;
                }
            }
            */

                /*
            double? lastOrderLat = null;
                double? lastOrderLng = null;
                var branch = _db.Branches.Where(p => p.Id == orderList[0].BranchId).FirstOrDefault();
                if (branch != null)
                {
                    lastOrderLat = branch.Lat;
                    lastOrderLng = branch.Lng;
                }
                
                List<Order> orders = orderList.Where(p => p.Status == OrderStatus.DELIVERED).OrderBy(p => p.DeliveredAt).ToList();
                if (vehId != null)
                {
                    Vehicle veh = _db.Vehicles.Where(p => p.Id == vehId).FirstOrDefault();
                    lastOrderLat = veh.OriginLat;
                    lastOrderLng = veh.originLng;
                    //orders = orders.Where(p => p.VehicleId == vehId).ToList();
                }
                if (driverId != null)
                {
                    orders = orders.Where(p => p.DriverId == driverId).ToList();
                }
                if (lastOrderLat != null && lastOrderLng != null)
                {
                    foreach (var order in orders)
                    {

                        //if (order.TripOriginLat != null && order.TripOriginLng != null && order.TripDestinationLat != null && order.TripDestinationLng != null)
                        {
                            double distance = DistanceBetweenPlacesinMtr((double)lastOrderLat, (double)lastOrderLng, order.Address.Lat, order.Address.Lng);
                            totDistance += (long)distance;
                        }
                        lastOrderLat = order.Address.Lat;
                        lastOrderLng = order.Address.Lng;
                    }
                }
                
            }
            catch(Exception ex)
            {

            }
            
            return totDistance;
        }*/


        public async Task<DriverActivitySummaryVM> GetActivitySummary(int? tenantId, int? branchId, int? driverId, DateTime fromDate, DateTime toDate)
        {
            DriverActivitySummaryVM activitySummary = new DriverActivitySummaryVM() 
            { 
                WorkEndTime = null,
            };

            long totTime = 0;
            long loginStateDuration = 0;
            long logoutStateDuration = 0;
            long breakStateDuration = 0; 
            long resumeStateDuration = 0;
            try
            {
                List<DriverActivity> activities = await _db.DriverActivities
                    .Include(p => p.Vehicle)
                    .Include(p => p.User)
                    .ThenInclude(p => p.Profile)
                    .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                if (driverId != null)
                {
                    activities = activities.Where(p => p.UserId == driverId).ToList();
                }

                if (tenantId != null)
                {
                    activities = activities.Where(p => p.User.TenantId == tenantId).ToList();
                }

                if (branchId != null)
                {
                    activities = activities.Where(p => p.User.BranchId == branchId).ToList();
                }

                List<DriverActivityModel> loginStateActivityList = new List<DriverActivityModel>();
                DriverActivity lastActivity = null;
                foreach (var activity in activities)
                {
                    double timeDiffInMin = 0;
                    if (lastActivity == null)
                    {
                        lastActivity = activity;
                        // to get active time from start of day
                        if (activity.dls == DLoginState.LoggedOut || activity.dls == DLoginState.PauseJob)
                        {
                            timeDiffInMin = (activity.CreatedAt - fromDate).TotalMinutes;
                            totTime += (long)timeDiffInMin;
                            activitySummary.WorkStartTime = fromDate;
                        }
                        else if (activity.dls == DLoginState.LoggedIN || activity.dls == DLoginState.ResumeJob)
                        {
                            activitySummary.WorkStartTime = activity.CreatedAt;
                            totTime += (long)timeDiffInMin;
                        }
                        loginStateActivityList.Add(new DriverActivityModel()
                        { 
                            ActivityState = activity.das,
                            CreatedAt = activity.CreatedAt,
                            LoginState = activity.dls,
                            VehicleId = (int)activity.VehicleId,
                            VehicleRegNo = activity.Vehicle.RegNo,
                            DriverId = activity.UserId,
                            DriverName = activity.User.Profile.GetFullName()
                        });
                        continue;
                    }

                    timeDiffInMin = (activity.CreatedAt - lastActivity.CreatedAt).TotalMinutes;
                    if (lastActivity.dls == DLoginState.LoggedIN || lastActivity.dls == DLoginState.ResumeJob)
                    {
                        //if (activity.dls == DLoginState.LoggedOut || activity.dls == DLoginState.PauseJob)
                        //{
                            totTime += (long)timeDiffInMin;
                        //}
                    }

                    if (lastActivity.dls == DLoginState.LoggedIN)
                    {
                        loginStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.LoggedOut)
                    {
                        logoutStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.PauseJob)
                    {
                        breakStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.ResumeJob)
                    {
                        resumeStateDuration += (long)timeDiffInMin;
                    }

                    if (lastActivity.dls != activity.dls)
                    {
                        loginStateActivityList.Add(new DriverActivityModel()
                        {
                            ActivityState = activity.das,
                            CreatedAt = activity.CreatedAt,
                            LoginState = activity.dls,
                            VehicleId = (int)activity.VehicleId,
                            VehicleRegNo = activity.Vehicle.RegNo,
                            DriverId = activity.UserId,
                            DriverName = activity.User.Profile.GetFullName()
                        });
                    }
                    lastActivity = activity;

                }

                // to get active time upto now/end of day if driver has not loggedoff/breaked
                if (lastActivity != null)
                {
                    DateTime now = DateMgr.GetCurrentIndiaTime();
                    toDate = toDate > now ? now : toDate;
                    var timeDiffInMin = (toDate - lastActivity.CreatedAt).TotalMinutes;
                    if (lastActivity.dls == DLoginState.LoggedIN || lastActivity.dls == DLoginState.ResumeJob)
                    {
                        totTime += (long)timeDiffInMin;
                        if(toDate != now)
                        {
                            activitySummary.WorkEndTime = toDate;
                        }
                        
                    }
                    else
                    {
                        activitySummary.WorkEndTime = lastActivity.CreatedAt;
                    }
                    if (lastActivity.dls == DLoginState.LoggedIN)
                    {
                        loginStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.LoggedOut)
                    {
                        logoutStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.PauseJob)
                    {
                        breakStateDuration += (long)timeDiffInMin;
                    }
                    else if (lastActivity.dls == DLoginState.ResumeJob)
                    {
                        resumeStateDuration += (long)timeDiffInMin;
                    }
                }

                activitySummary.ActiveTimeInMin = totTime;
                activitySummary.ActivityList = loginStateActivityList;
                activitySummary.LoginStateSummaryList = new List<DriverLoginStateVM>
                {
                    new DriverLoginStateVM
                    {
                        LoginState = DLoginState.LoggedIN,
                        Count = loginStateActivityList.Where(p => p.LoginState == DLoginState.LoggedIN).Count(),
                        DurationInMin = loginStateDuration
                    },
                    new DriverLoginStateVM
                    {
                        LoginState = DLoginState.LoggedOut,
                        Count = loginStateActivityList.Where(p => p.LoginState == DLoginState.LoggedOut).Count(),
                        DurationInMin = logoutStateDuration
                    },
                    new DriverLoginStateVM
                    {
                        LoginState = DLoginState.PauseJob,
                        Count = loginStateActivityList.Where(p => p.LoginState == DLoginState.PauseJob).Count(),
                        DurationInMin = breakStateDuration
                    },
                    new DriverLoginStateVM
                    {
                        LoginState = DLoginState.ResumeJob,
                        Count = loginStateActivityList.Where(p => p.LoginState == DLoginState.ResumeJob).Count(),
                        DurationInMin = resumeStateDuration
                    }
                };
            }
            catch (Exception ex)
            {

            }
            return activitySummary;
        }

        public long GetActiveTimeInMin(int? vehId, DateTime fromDate, DateTime toDate)
        {
            long totTime = 0;
            try
            {
                
                List<DriverActivity> activities = _db.DriverActivities.Include(p => p.Vehicle).Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate).OrderBy(p => p.CreatedAt).ToList();
                if (vehId != null)
                {
                    Vehicle veh = _db.Vehicles.Where(p => p.Id == vehId).FirstOrDefault();
                    activities = activities.Where(p => p.VehicleId == vehId).ToList();
                }
                DriverActivity lastActivity = null;
                foreach (var activity in activities)
                {
                    if (lastActivity == null)
                    {
                        lastActivity = activity;
                        // to get active time from start of day
                        if (activity.dls == DLoginState.LoggedOut || activity.dls == DLoginState.PauseJob)
                        {
                            var timeDiffInMin = (activity.CreatedAt - fromDate).TotalMinutes;
                            totTime += (long)timeDiffInMin;
                        }
                        continue;
                    }

                    if (lastActivity.dls == DLoginState.LoggedIN || lastActivity.dls == DLoginState.ResumeJob)
                    {
                        if (activity.dls == DLoginState.LoggedOut || activity.dls == DLoginState.PauseJob)
                        {
                            var timeDiffInMin = (activity.CreatedAt - lastActivity.CreatedAt).TotalMinutes;
                            totTime += (long)timeDiffInMin;
                        }
                    }
                    lastActivity = activity;

                }

                // to get active time upto now/end of day if driver has not loggedoff/breaked
                if (lastActivity != null)
                {
                    if (lastActivity.dls == DLoginState.LoggedIN || lastActivity.dls == DLoginState.ResumeJob)
                    {
                        DateTime now = DateMgr.GetCurrentIndiaTime();
                        toDate = toDate > now ? now : toDate;
                        var timeDiffInMin = (toDate - lastActivity.CreatedAt).TotalMinutes;
                        totTime += (long)timeDiffInMin;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
            return totTime;
        }
    }
}

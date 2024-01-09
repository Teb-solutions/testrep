using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Profiles.API.Models;
using Profiles.API.ViewModels.Relaypoint;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using Profiles.API.ViewModels.CartAggregate;
using Microsoft.AspNetCore.Mvc.Rendering;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.DriverPickup;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.Services;
using EasyGas.Shared.Formatters;
using Profiles.API.ViewModels.BusinessEntity;

namespace EasyGas.Services.Profiles.Queries
{
    public class BusinessEntityQueries : IBusinessEntityQueries
    {
        private readonly ProfilesDbContext _db;
        private readonly ILocationService _locationService;
        private readonly IVehicleQueries _vehQueries;

        public BusinessEntityQueries(ProfilesDbContext ctx, ILocationService locationService, IVehicleQueries vehQueries)
        {
            _db = ctx;
            _locationService = locationService;
            _vehQueries = vehQueries;
        }

        public async Task<List<BusinessEntityModel>> GetAllList(BusinessEntityType? type = null, bool? isActive = null, int? tenantId = null, int? branchId = null, int? parentId = null, double? originLat = null, double? originLng = null, double? radius = 999999999)
        {
            List<BusinessEntityModel> list = new List<BusinessEntityModel>();

            var query = _db.BusinessEntities.AsQueryable();
            if (type != null)
            {
                query = query.Where(p => p.Type == type);
            }

            if (tenantId != null)
                {
                    query = query.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    query = query.Where(p => p.BranchId == branchId);
                }
                if (parentId != null)
                {
                    query = query.Where(p => p.ParentBusinessEntityId == parentId);
                }

            if (isActive != null)
            {
                query = query.Where(p => p.IsActive == isActive);
            }

            Point origin = null;
            if (originLat != null && originLng != null)
            {
                origin = new Point((double)originLng, (double)originLat) { SRID = 4326 };
                if (radius != null)
                {
                    query = query.Where(p => (p.GeoLocation.Distance(origin) <= radius));
                }

                query = query.OrderBy(p => p.GeoLocation.Distance(origin));
            }

            var data = await query
                .Include(p => p.ParentBusinessEntity)
                .Include(p => p.Branch)
                .Select(entity => 
            new BusinessEntityModel {
                Id = entity.Id,
                Type = entity.Type,
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                BranchName = entity.Branch.Name,
                Code = entity.Code,
                Name = entity.Name,
                MobileNumber = entity.MobileNumber,
                Email = entity.Email,
                WorkingStartDay = entity.WorkingStartDay,
                WorkingEndDay = entity.WorkingEndDay,
                WorkingStartTime = entity.WorkingStartTime,
                WorkingEndTime = entity.WorkingEndTime,
                Location = entity.Location,
                Lat = entity.Lat,
                Lng = entity.Lng,
                DistanceFromOrigin = (originLat != null && originLng != null) ? (entity.GeoLocation.Distance(origin) / 1000) : 0,
                Landmark = entity.Landmark,
                Details = entity.Details,
                PinCode = entity.PinCode,
                State = entity.State,
                ParentBusinessEntityId = entity.ParentBusinessEntityId,
                GSTN = entity.GSTN,
                PAN = entity.PAN,
                PaymentNumber = entity.PaymentNumber,
                UPIQRCodeImageUrl = entity.UPIQRCodeImageUrl,
                Rating = entity.Rating,
                IsActive = entity.IsActive,
                ParentBusinessEntityName = entity.ParentBusinessEntity != null ? entity.ParentBusinessEntity.Name : ""
            }).ToListAsync();

            return data;
        }

        public async Task<BusinessEntityModel> GetDetailsById(int id)
        {
            BusinessEntity entity = await _db.BusinessEntities
                .Include(p => p.Branch)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                BusinessEntityModel model = MapToModel(entity);
                return model;
            }
            else return null;
        }


        #region relaypoint

        public async Task<List<CartRelaypoint>> GetRelaypointListForCustomerCart(int? branchId, double? originLat, double? originLng, double? radius = null)
        {
            List<CartRelaypoint> list = new List<CartRelaypoint>();

            var query = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Relaypoint && p.IsActive).AsQueryable();

            if (branchId != null)
            {
                query = query.Where(p => p.BranchId == branchId);
            }

            if (radius == null)
            {
                radius = 9999999;
            }

            Point origin = null;

            if (originLat == null || originLng == null)
            {
                if (branchId != null)
                {
                    var branch = await _db.Branches.Where(p => p.Id == branchId).FirstOrDefaultAsync();
                    if (branch != null)
                    {
                        originLat = branch.Lat;
                        originLng = branch.Lng;
                    }
                }
            }

            if (originLat != null && originLng != null)
            {
                origin = new Point((double)originLng, (double)originLat) { SRID = 4326 };
                if (radius != null)
                {
                    query = query.Where(p => (p.GeoLocation.Distance(origin) <= radius));
                }

                query = query.OrderBy(p => p.GeoLocation.Distance(origin));
            }

            var data = await query
                .Include(p => p.ParentBusinessEntity)
                .Include(p => p.Timings)
                .Select(entity =>
            new CartRelaypoint
            {
                Id = entity.Id,
                BranchId = entity.BranchId,
                Code = entity.Code,
                Name = entity.Name,
                MobileNumber = entity.MobileNumber,
                Email = entity.Email,
                //WorkingDay = entity.WorkingStartDay.ToString().Substring(0, 3) + " - " + entity.WorkingEndDay.ToString().Substring(0, 3),
                WorkingTime = entity.WorkingStartTime + " - " + entity.WorkingEndTime,
                Location = entity.Location,
                Lat = entity.Lat,
                Lng = entity.Lng,
                DistanceFromOrigin = (originLat != null && originLng != null) ? Math.Round((entity.GeoLocation.Distance(origin) / 1000)) : 0,
                Rating = entity.Rating,
                CoverImageUrl = "https://totalenergies.com/sites/g/files/nytnzq121/files/images/2021-10/synova.jpg",
                ProfileImageUrl = "https://totalenergies.com/pr_assets/images/logo/logo.png",
                WorkingDaysList = entity.Timings.Select(timing => new WorkingDaysModel
                {
                    Day = timing.Day,
                    IsActive = timing.IsActive
                }).ToList()
            })
            .OrderBy(p => p.DistanceFromOrigin)
            .ToListAsync();

            return data;
        }

        public async Task<List<DriverPickupRelaypointDetails>> GetRelaypointListForDriverPickup(int driverId, int orderId, double? originLat, double? originLng, double? radius = null)
        {
            List<DriverPickupRelaypointDetails> list = new List<DriverPickupRelaypointDetails>();
            var driver = _db.Users.Include(p => p.Branch).Where(p => p.Id == driverId && p.Type == UserType.DRIVER).FirstOrDefault();
            if (driver == null)
            {
                return list;
            }

            //TODO show only relaypoints which has enough stock for items in order

            var query = _db.BusinessEntities
                .Where(p => p.Type == BusinessEntityType.Relaypoint && p.IsActive && p.BranchId == driver.BranchId).AsQueryable();

            if (radius == null)
            {
                radius = 9999999;
            }

            Point origin = null;

            if (originLat == null || originLng == null)
            {
                if (driver.Branch != null)
                {
                    originLat = driver.Branch.Lat;
                    originLng = driver.Branch.Lng;
                }
            }

            if (originLat != null && originLng != null)
            {
                origin = new Point((double)originLng, (double)originLat) { SRID = 4326 };
                if (radius != null)
                {
                    query = query.Where(p => (p.GeoLocation.Distance(origin) <= radius));
                }

                query = query.OrderBy(p => p.GeoLocation.Distance(origin));
            }

            var data = await query.Include(p => p.ParentBusinessEntity).Select(entity =>
            new DriverPickupRelaypointDetails
            {
                Id = entity.Id,
                BranchId = entity.BranchId,
                Code = entity.Code,
                Name = entity.Name,
                MobileNumber = entity.MobileNumber,
                Email = entity.Email,
                //WorkingDay = entity.WorkingStartDay.ToString().Substring(0, 3) + " - " + entity.WorkingEndDay.ToString().Substring(0, 3),
                WorkingTime = entity.WorkingStartTime + " - " + entity.WorkingEndTime,
                Location = entity.Location,
                Lat = entity.Lat,
                Lng = entity.Lng,
                DistanceFromOrigin = (originLat != null && originLng != null) ? Math.Round((entity.GeoLocation.Distance(origin) / 1000)) : 0,
                Rating = entity.Rating,
                CoverImageUrl = "https://totalenergies.com/sites/g/files/nytnzq121/files/images/2021-10/synova.jpg",
                ProfileImageUrl = "https://totalenergies.com/pr_assets/images/logo/logo.png"
            })
                .OrderBy(p => p.DistanceFromOrigin)
                .ToListAsync();

            return data;
        }

        public async Task<DriverPickupRelaypointDetails> GetRelaypointDetailsForDriverPickup(int relaypointId)
        {
            var relaypoint = await _db.BusinessEntities
                .Where(p => p.Type == BusinessEntityType.Relaypoint && p.Id == relaypointId)
                .Select(entity =>
                    new DriverPickupRelaypointDetails
                    {
                        Id = entity.Id,
                        BranchId = entity.BranchId,
                        Code = entity.Code,
                        Name = entity.Name,
                        MobileNumber = entity.MobileNumber,
                        Email = entity.Email,
                        //WorkingDay = entity.WorkingStartDay.ToString().Substring(0, 3) + " - " + entity.WorkingEndDay.ToString().Substring(0, 3),
                        WorkingTime = entity.WorkingStartTime + " - " + entity.WorkingEndTime,
                        Location = entity.Location,
                        Lat = entity.Lat,
                        Lng = entity.Lng,
                        Rating = entity.Rating,
                        CoverImageUrl = "https://totalenergies.com/sites/g/files/nytnzq121/files/images/2021-10/synova.jpg",
                        ProfileImageUrl = "https://totalenergies.com/pr_assets/images/logo/logo.png"
                    })
                .FirstOrDefaultAsync();

            return relaypoint;
        }

        public async Task<List<SelectListItem>> GetRelaypointSelectList(int? tenantId, int? branchId)
        {
            var query = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Relaypoint && p.IsActive).AsQueryable();

            if (tenantId != null)
            {
                query = query.Where(p => p.TenantId == tenantId);
            }

            if (branchId != null)
            {
                query = query.Where(p => p.BranchId == branchId);
            }

            var list = await query.Select(p => new SelectListItem()
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).ToListAsync();

            return list;
        }

        public async Task<User> GetRelaypointAdminUser(int relaypointId)
        {
            var relaypointUser = await _db.Users
                .Include(p => p.Profile)
                .Where(p => p.BusinessEntityId == relaypointId && p.Type == UserType.RELAY_POINT)
                .FirstOrDefaultAsync();

            return relaypointUser;
        }

        public async Task<RelaypointProfile> GetRelaypointProfile(int relaypointId)
        {
            var bussinessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId)
                .Select(p => new RelaypointProfile()
                {
                    Name = p.Name,
                    MobileNumber = p.MobileNumber,
                    Email = p.Email,
                    Rating = p.Rating
                })
                .FirstOrDefaultAsync();

            return bussinessEntity;
        }

        public async Task<UpdateRelaypointAddress> GetRelaypointAddress(int relaypointId)
        {
            var bussinessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId)
                .Select(p => new UpdateRelaypointAddress()
                {
                    Details = p.Details,
                    Landmark = p.Landmark,
                    Lat = p.Lat,
                    Lng = p.Lng,
                    Location = p.Location,
                    PinCode = p.PinCode
                })
                .FirstOrDefaultAsync();

            return bussinessEntity;
        }

        public async Task<List<WorkingDaysModel>> GetRelaypointWorkingDays(int relaypointId)
        {
            var workingDays = await _db.BusinessEntityTimings
                .Where(p => p.BusinessEntityId == relaypointId)
                .Select(p => new WorkingDaysModel() 
                {
                    Day = p.Day,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return workingDays;
        }

        public async Task<UpdateWorkingTimeRequest> GetRelaypointWorkingTime(int relaypointId)
        {
            var bussinessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId)
                .Select(p => new UpdateWorkingTimeRequest()
                {
                    StartTime = p.WorkingStartTime,
                    EndTime = p.WorkingEndTime
                })
                .FirstOrDefaultAsync();

            return bussinessEntity;
        }

        #endregion

        #region businessEntity general

        public async Task<List<SelectListItem>> GetSelectList(BusinessEntityType type, int? tenantId, int? branchId)
        {
            var query = _db.BusinessEntities.Where(p => p.Type == type && p.IsActive).AsQueryable();

            if (tenantId != null)
            {
                query = query.Where(p => p.TenantId == tenantId);
            }

            if (branchId != null)
            {
                query = query.Where(p => p.BranchId == branchId);
            }

            var list = await query.Select(p => new SelectListItem()
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).ToListAsync();

            return list;
        }

        public async Task<DistributorProfile> GetProfile(int businessEntityId)
        {
            var bussinessEntity = await _db.BusinessEntities
                .Where(p => p.Id == businessEntityId)
                .Select(p => new DistributorProfile()
                {
                    Name = p.Name,
                    Code = p.Code,
                    MobileNumber = p.MobileNumber,
                    Email = p.Email,
                    Rating = p.Rating,
                    GSTN = p.GSTN,
                    PAN = p.PAN,
                    UpiPaymentNumber = p.PaymentNumber
                })
                .FirstOrDefaultAsync();

            return bussinessEntity;
        }

        public async Task<UpdateDistributorAddress> GetAddress(int businessEntityId)
        {
            var bussinessEntity = await _db.BusinessEntities
                .Where(p => p.Id == businessEntityId)
                .Select(p => new UpdateDistributorAddress()
                {
                    Details = p.Details,
                    Landmark = p.Landmark,
                    Lat = p.Lat,
                    Lng = p.Lng,
                    Location = p.Location,
                    PinCode = p.PinCode
                })
                .FirstOrDefaultAsync();

            return bussinessEntity;
        }

        public async Task<User> GetAdminUser(int id, UserType type)
        {
            var user = await _db.Users
                .Include(p => p.Profile)
                .Where(p => p.BusinessEntityId == id && p.Type == type)
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<DistributorDashboardModel> GetBusinessEntityDashboard(int id, int userId)
        {
            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                return null;
            }

            DistributorDashboardModel response = new DistributorDashboardModel();
            DateTime today = DateMgr.GetCurrentIndiaTime().Date;
            DateTime now = DateMgr.GetCurrentIndiaTime();

            #region Vehicles
            var vehicles = await _vehQueries.GetAllList(businessEntity.TenantId, businessEntity.BranchId, businessEntity.Id, true);
            List<VehicleLocationsViewModel> vehLocList = await _locationService.GetVehicleLocations(businessEntity.TenantId);
            List<DistributorVehicleModel> vehList = new List<DistributorVehicleModel>();
            foreach (var veh in vehicles)
            {
                DistributorVehicleModel vehModel = DistributorVehicleModel.FromVehicleModel(veh);
                var vehLoc = vehLocList.Where(p => p.VehicleId == veh.Id).FirstOrDefault();
                if (vehLoc != null)
                {
                    vehModel.LastLocationLat = vehLoc.Lat;
                    vehModel.LastLocationLng = vehLoc.Lng;
                    vehModel.LastLocationUpdatedAt = vehLoc.CreatedAt;
                }
                vehList.Add(vehModel);
            }

            response.Vehicles = vehList;
            response.TotalVehicles = vehicles.Where(p => p.IsActive).Count();
            response.VehiclesActive = vehicles.Where(p => p.IsActive && p.State == VehicleState.ReadyForWork).Count();
            response.VehiclesInBreak = vehicles.Where(p => p.IsActive && p.State == VehicleState.Break).Count();
            response.VehiclesLogoff = vehicles.Where(p => p.IsActive && p.State == VehicleState.OutFromWork).Count();

            #endregion

            var customers = await _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.CUSTOMER && p.BusinessEntityId == id).ToListAsync();
           
            response.TotalCustomers = customers.Count();
            response.NewCustomersToday = customers.Where(p => p.BusinessEntityAttachedAt >= today).Count();

            response.RecentCustomers = customers
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new DistributorCustomerModel
                {
                    CreatedAt = p.CreatedAt,
                    Email = p.Profile.Email,
                    FirstName = p.Profile.FirstName,
                    LastName = p.Profile.LastName,
                    LastLoginAt = p.LastLogin,
                    IsReferredByDistributor = p.Profile.ReferredByUserId == userId,
                    Mobile = p.Profile.Mobile,
                    Source = p.Profile.Source
                })
                .ToList();

            return response;
        }

        public async Task<CreateBusinessEntityRequest> GetDetailsByIdForUpdate(int id)
        {
            var entity = await _db.BusinessEntities
                .Include(p => p.ParentBusinessEntity)
                .Include(p => p.Timings)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                CreateBusinessEntityRequest model = new CreateBusinessEntityRequest 
                {
                    Id = entity.Id,
                    BranchId = entity.BranchId,
                    Code = entity.Code,
                    Email = entity.Email,
                    Details = entity.Details,
                    GSTN = entity.GSTN,
                    IsActive = entity.IsActive,
                    Landmark = entity.Landmark,
                    Lat = entity.Lat,
                    Lng = entity.Lng,
                    Location = entity.Location,
                    MobileNumber = entity.MobileNumber,
                    Name = entity.Name,
                    PAN = entity.PAN,
                    ParentBusinessEntityId = entity.ParentBusinessEntityId,
                    PaymentNumber = entity.PaymentNumber,
                    PinCode = entity.PinCode,
                    State = entity.State,
                    Type = entity.Type, 
                    UPIQRCodeImageUrl = entity.UPIQRCodeImageUrl,
                    WorkingStartTime = entity.WorkingStartTime,
                    WorkingEndTime = entity.WorkingEndTime,
                    Rating = entity.Rating,
                    WorkingDaysList = entity.Timings.Select(p => new WorkingDaysModel
                    {
                        Day = p.Day,
                        IsActive = p.IsActive
                    }).ToList(),
                    
                };
                return model;
            }
            else return null;
        }

        #endregion

        #region Device

        public async Task<List<Device>> GetDevices(int entityId)
        {
            var devices = await _db.Devices
                .Include(p => p.ChildDevices)
                .Where(x => x.BusinessEntityId == entityId)
                .ToListAsync();

            return devices;
        }

        public async Task<Device> GetDeviceById(int id)
        {
            Device device = await _db.Devices
                .Include(p => p.BusinessEntity)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (device != null)
            {
                return device;
            }

            else return null;
        }

        public async Task<List<Device>> GetDevicesByParentId(int id)
        {
            var devices = await _db.Devices
                .Include(p => p.BusinessEntity)
                .Where(p => p.ParentDeviceId == id)
                .ToListAsync();

            return devices;
        }

        #endregion

        private BusinessEntityModel MapToModel(BusinessEntity entity)
        {
            var model = new BusinessEntityModel()
            {
                Id = entity.Id,
                Type = entity.Type,
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                BranchName = entity.Branch?.Name,
                Code = entity.Code,
                Name = entity.Name,
                MobileNumber = entity.MobileNumber,
                Email = entity.Email,
                WorkingStartDay = entity.WorkingStartDay,
                WorkingEndDay = entity.WorkingEndDay,
                WorkingStartTime = entity.WorkingStartTime,
                WorkingEndTime = entity.WorkingEndTime,
                Location = entity.Location,
                Lat = entity.Lat,
                Lng = entity.Lng,
                //GeoLocation = entity.GeoLocation,
                Landmark = entity.Landmark,
                Details = entity.Details,
                PinCode = entity.PinCode,
                State = entity.State,
                ParentBusinessEntityId = entity.ParentBusinessEntityId,

                GSTN = entity.GSTN,
                PAN = entity.PAN,
                PaymentNumber = entity.PaymentNumber,
                UPIQRCodeImageUrl = entity.UPIQRCodeImageUrl,

                Rating = entity.Rating,
                IsActive = entity.IsActive,
            };

            if (entity.ParentBusinessEntity != null)
            {
                 model.ParentBusinessEntityName = entity.ParentBusinessEntity.Name;
            }

            return model;
        }
    }
}

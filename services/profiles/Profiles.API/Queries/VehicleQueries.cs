using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyGas.Shared.Formatters;

namespace EasyGas.Services.Profiles.Queries
{
    public class VehicleQueries : IVehicleQueries
    {
        private readonly ProfilesDbContext _db;

        public VehicleQueries(ProfilesDbContext ctx)
        {
            _db = ctx;
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync(int? tenantId, int? branchId, int? distributorId)
        {
            var vehQuery = _db.Vehicles.AsQueryable();
            if (tenantId != null)
            {
                vehQuery = vehQuery.Where(p => p.BusinessEntity.TenantId == tenantId);
            }
            if (branchId != null)
            {
                vehQuery = vehQuery.Where(p => p.BusinessEntity.BranchId == branchId);
            }
            if (distributorId != null)
            {
                vehQuery = vehQuery.Where(p => p.BusinessEntityId == distributorId);
            }
            var data = await vehQuery.ToListAsync();
            return data;
        }

        public async Task<List<VehicleModel>> GetAllList(int? tenantId, int? branchId, int? businessEntityId, bool includeDriverActivity = false)
        {
            List<VehicleModel> list = new List<VehicleModel>();
            try
            {
                var vehQuery = _db.Vehicles.AsQueryable();
                if (tenantId != null)
                {
                    vehQuery = vehQuery.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    vehQuery = vehQuery.Where(p => p.BranchId == branchId);
                }
                if (businessEntityId != null)
                {
                    vehQuery = vehQuery.Where(p => p.BusinessEntityId == businessEntityId);
                }
                var data = await vehQuery
                    .Include(p => p.Driver)
                    .Include(p => p.Driver.Profile)
                    .Include(p => p.BusinessEntity)
                    .ToListAsync();

                List<DriverActivity> driverActivities = new List<DriverActivity>();
                var now = DateMgr.GetCurrentIndiaTime();

                if (includeDriverActivity && data.Count > 0)
                {
                    var driverIdList = data.Where(p => p.DriverId != null).Select(p => p.DriverId).AsEnumerable();
                    driverActivities = await _db.DriverActivities
                        .Where(p => driverIdList.Contains(p.UserId))
                        .ToListAsync();
                }

                foreach (var vehicle in data)
                {
                    VehicleModel veh = MapToVehicleModel(vehicle);
                    if (includeDriverActivity)
                    {
                        var driverActivity = driverActivities.Where(p => p.UserId == vehicle.DriverId).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (driverActivity != null)
                        {
                            veh.DriverLoginState = driverActivity.dls;
                            veh.DriverActivityState = driverActivity.das;
                            veh.DriverLoginStateTime = driverActivity.CreatedAt;
                            veh.DriverActivityStateTime = driverActivity.CreatedAt;
                            veh.DriverLoginStateTimeAgo = (int)now.Subtract(driverActivity.CreatedAt).TotalMinutes;
                            veh.DriverActivityStateTimeAgo = (int)now.Subtract(driverActivity.CreatedAt).TotalMinutes;
                        }
                    }
                    list.Add(veh);
                }
                return list;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return list;
            }
        }

        public async Task<List<BuildingBlocks.EventBus.Events.VehicleModel>> GetAllListForPlanning(int? tenantId, int? branchId, int? distributorId, bool includeDriverActivity = false)
        {
            List<BuildingBlocks.EventBus.Events.VehicleModel> list = new List<BuildingBlocks.EventBus.Events.VehicleModel>();
            var vehQuery = _db.Vehicles.AsQueryable();
            if (tenantId != null)
            {
                vehQuery = vehQuery.Where(p => p.TenantId == tenantId);
            }
            if (branchId != null)
            {
                vehQuery = vehQuery.Where(p => p.BranchId == branchId);
            }
            if (distributorId != null)
            {
                vehQuery = vehQuery.Where(p => p.BusinessEntityId == distributorId);
            }
            var data = await vehQuery
                .Include(p => p.Driver)
                .Include(p => p.Driver.Profile)
                .Include(p => p.BusinessEntity)
                .ToListAsync();

            List<DriverActivity> driverActivities = new List<DriverActivity>();
            var now = DateMgr.GetCurrentIndiaTime();

            if (includeDriverActivity && data.Count > 0)
            {
                var driverIdList = data.Where(p => p.DriverId != null).Select(p => p.DriverId).AsEnumerable();
                driverActivities = await _db.DriverActivities
                    .Where(p => driverIdList.Contains(p.UserId))
                    .ToListAsync();
            }

            foreach (var vehicle in data)
            {
                BuildingBlocks.EventBus.Events.VehicleModel veh = MapToEventVehicleModel(vehicle);
                if (includeDriverActivity)
                {
                    var driverActivity = driverActivities.Where(p => p.UserId == vehicle.DriverId).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (driverActivity != null)
                    {
                        veh.LoggedOut = driverActivity.dls == DLoginState.LoggedOut;
                        veh.PauseJob = driverActivity.dls == DLoginState.PauseJob;
                    }
                }
                list.Add(veh);
            }
            return list;
        }

        public async Task<VehicleModel> GetDetails(int id, bool includeDriverActivity = false )
        {
            Vehicle vehicle = await _db.Vehicles
                .Include(p => p.Driver)
                .ThenInclude(p => p.Profile)
                .Include(p => p.BusinessEntity)
                .SingleOrDefaultAsync(x => x.Id == id);
            
            if (vehicle != null)
            {
                VehicleModel veh = MapToVehicleModel(vehicle);
                if (includeDriverActivity && vehicle.DriverId != null)
                {
                    var driverActivity = _db.DriverActivities.Where(p => p.UserId == vehicle.DriverId).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (driverActivity != null)
                    {
                        var now = DateMgr.GetCurrentIndiaTime();
                        veh.DriverLoginState = driverActivity.dls;
                        veh.DriverActivityState = driverActivity.das;
                        veh.DriverLoginStateTime = driverActivity.CreatedAt;
                        veh.DriverActivityStateTime = driverActivity.CreatedAt;
                        veh.DriverLoginStateTimeAgo = (int)now.Subtract(driverActivity.CreatedAt).TotalMinutes;
                        veh.DriverActivityStateTimeAgo = (int)now.Subtract(driverActivity.CreatedAt).TotalMinutes;
                    }
                }
                
                return veh;
            }
            return null;
        }



        public int GetDriversVehicleId(int driverId)
        {
            var vehicle =_db.Vehicles.Where(p => p.DriverId == driverId).FirstOrDefault();
            if (vehicle != null)
            {
                return vehicle.Id;
            }
            return 0;
        }

        private VehicleModel MapToVehicleModel(Vehicle vehicle)
        {
            VehicleModel veh = new VehicleModel();
            veh.TenantId = vehicle.TenantId;
            veh.BranchId = vehicle.BranchId;
            veh.RegNo = vehicle.RegNo;
            veh.Id = vehicle.Id;
            veh.DestinationLat = vehicle.DestinationLat;
            veh.DestinationLng = vehicle.DestinationLng;
            veh.OriginLat = vehicle.OriginLat;
            veh.OriginLng = vehicle.OriginLng;

            if (vehicle.Driver != null)
            {
                if (vehicle.Driver.Profile != null)
                {
                    veh.DriverId = vehicle.DriverId;
                    veh.DriverName = vehicle.Driver.Profile.GetFullName();
                    veh.DriverMobile = vehicle.Driver.Profile.Mobile;
                    veh.DriverDeviceId = vehicle.Driver.Profile.DeviceId;
                }
            }
            if (vehicle.BusinessEntity != null)
            {
                veh.BusinessEntityId = vehicle.BusinessEntityId;
                veh.BusinessEntityName = vehicle.BusinessEntity.Name;
                veh.BusinessEntityMobile = vehicle.BusinessEntity.MobileNumber;
                veh.BusinessEntityUpiPaymentNumber = vehicle.BusinessEntity.PaymentNumber;
                veh.BusinessEntityUpiQrCodeUrl = vehicle.BusinessEntity.UPIQRCodeImageUrl;
                veh.BusinessEntityLat = vehicle.BusinessEntity.Lat;
                veh.BusinessEntityLng = vehicle.BusinessEntity.Lng;
            }
            veh.IsActive = vehicle.IsActive;
            veh.State = vehicle.State;
            return veh;
        }

        private BuildingBlocks.EventBus.Events.VehicleModel MapToEventVehicleModel(Vehicle vehicle)
        {
            BuildingBlocks.EventBus.Events.VehicleModel veh = new BuildingBlocks.EventBus.Events.VehicleModel();
            veh.RegNo = vehicle.RegNo;
            veh.Id = vehicle.Id;
            veh.DestinationLat = vehicle.DestinationLat;
            veh.DestinationLng = vehicle.DestinationLng;
            veh.OriginLat = vehicle.OriginLat;
            veh.OriginLng = vehicle.OriginLng;
            if (vehicle.Driver != null)
            {
                if (vehicle.Driver.Profile != null)
                {
                    veh.DriverId = vehicle.DriverId;
                    veh.DriverName = vehicle.Driver.Profile.GetFullName();
                    veh.DriverMobile = vehicle.Driver.Profile.Mobile;
                }
            }

            if (vehicle.BusinessEntity != null)
            {
                veh.BusinessEntityId = vehicle.BusinessEntityId;
                veh.BusinessEntityName = vehicle.BusinessEntity.Name;
                veh.BusinessEntityMobile = vehicle.BusinessEntity.MobileNumber;
                veh.BusinessEntityUpiPaymentNumber = vehicle.BusinessEntity.PaymentNumber;
                veh.BusinessEntityUpiQrCodeUrl = vehicle.BusinessEntity.UPIQRCodeImageUrl;
                veh.BusinessEntityLat = vehicle.BusinessEntity.Lat;
                veh.BusinessEntityLng = vehicle.BusinessEntity.Lng;
            }

            veh.IsActive = vehicle.IsActive;
            return veh;
        }
    }
}

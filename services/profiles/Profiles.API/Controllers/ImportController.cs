using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using EasyGas.Services.Profiles.Models.AdminWebsiteVM;
using EasyGas.Services.Profiles.BizLogic;
using Microsoft.AspNetCore.Authorization;
using Profiles.API.Controllers;
using System.Net;
using Profiles.API.Infrastructure.Services;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels;
using Profiles.API.Models.DriverPickupOrder;
using Profiles.API.ViewModels.Relaypoint;
using Profiles.API.Models;
using Profiles.API.ViewModels.CartAggregate;
using Profiles.API.ViewModels.DriverPickup;
using Profiles.API.IntegrationEvents;
using EasyGas.BuildingBlocks.EventBus.Events;
using EventVehicleModel = EasyGas.BuildingBlocks.EventBus.Events.VehicleModel;
using EasyGas.Shared.Models;
using Microsoft.Extensions.Logging;
using EasyGas.Services.Profiles.Services;
using NetTopologySuite;
using ImportUser = Profiles.API.ViewModels.Import.User;
using NetTopologySuite.Geometries;
using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using Microsoft.EntityFrameworkCore;
using Profiles.API.ViewModels.Import;
using User = EasyGas.Services.Profiles.Models.User;
using UserProfile = EasyGas.Services.Profiles.Models.UserProfile;
using Vehicle = EasyGas.Services.Profiles.Models.Vehicle;
using VehicleState = EasyGas.Services.Profiles.Models.VehicleState;
using UserAddress = EasyGas.Services.Profiles.Models.UserAddress;
using Gender = EasyGas.Services.Profiles.Models.Gender;

namespace EasyGas.Services.Profiles.Controllers
{
    [Authorize(Roles = "TENANT_ADMIN")]
    [Route("api/v1/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class ImportController : BaseApiController
    {
        private readonly IImportService _importService;
        private readonly ILogger _logger;
        private ProfilesDbContext _db;

        public ImportController(
             ICommandBus bus,  ILogger<ImportController> logger,
            ProfilesDbContext db, IImportService importService)
            : base(bus)
        {
            _logger = logger;
            _importService = importService;
            _db = db;
        }


        [HttpGet]
        [Route("distributors")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Distributors()
        {
            try
            {
                var users = await _importService.GetDistributors();
                if (users == null)
                {
                    return NotFound();
                }

                var distributors = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Distributor).ToList();
                var role = _db.Roles.Where(p => p.Name == RoleNames.DISTRIBUTOR_ADMIN).FirstOrDefault();
                if (role == null)
                {
                    return BadRequest($"Distributor Role is not set.");
                }

                foreach (var user in users)
                {
                    if (user.Addresses == null || user.Addresses.Count == 0) 
                    { 
                        continue;
                    }

                    var distributor = distributors.Where(p => p.MobileNumber == user.Profile.Mobile).FirstOrDefault();
                    if (distributor == null)
                    {
                        distributor = CreateBusinessEntityFromDistributorModel(user);
                        _db.BusinessEntities.Add(distributor);

                        var distributorUser = CreateDistributorUserFromModel(user);
                        distributorUser.Password = Helper.HashPassword(distributorUser.Password);

                        var distributorUserProfile = CreateDistributorProfileFromModel(user);
                        distributorUser.Profile = distributorUserProfile;
                        distributorUser.Roles.Add(new UserRole(role.Id));
                        distributorUser.BusinessEntity = distributor;

                        _db.Users.Add(distributorUser);
                    }
                    else
                    {

                    }

                    //await _db.SaveChangesAsync();
                }
                await _db.SaveChangesAsync();

                return Ok(users.Count + " distributors added");
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("pincodes")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Pincodes()
        {
            try
            {
                var importPincodes = await _importService.GetPincodes();
                if (importPincodes == null)
                {
                    return NotFound();
                }

                var pincodes = _db.Pincodes.ToList();

                foreach (var importPincode in importPincodes)
                {
                    var existingPincode = pincodes.Where(p => p.Code == importPincode.Code).FirstOrDefault();
                    if (existingPincode != null)
                    {
                        //existingPincode.IsActive = importPincode.IsActive;
                        //existingPincode.EnabledBy = importPincode.EnabledBy;
                        //existingPincode.DisabledBy = importPincode.DisabledBy;
                        existingPincode.CreatedAt = importPincode.CreatedAt;
                        existingPincode.UpdatedAt = importPincode.UpdatedAt;
                        existingPincode.UpdatedBy = importPincode.UpdatedBy;
                        existingPincode.CreatedBy = importPincode.CreatedBy;
                        //existingPincode.IsDeleted = importPincode.IsDeleted;
                        _db.Entry(existingPincode).State = EntityState.Modified;
                    }
                    else
                    {
                        existingPincode = new Pincode();
                        existingPincode.Code = importPincode.Code;
                        existingPincode.TenantId = 1;
                        existingPincode.BranchId = importPincode.BranchId;
                        existingPincode.CreatedAt = importPincode.CreatedAt;
                        existingPincode.CreatedBy = importPincode.CreatedBy;
                        existingPincode.IsDeleted = importPincode.IsDeleted;
                        existingPincode.IsActive = importPincode.IsActive;
                        existingPincode.EnabledBy = importPincode.EnabledBy;
                        existingPincode.DisabledBy = importPincode.DisabledBy;
                        existingPincode.UpdatedAt = importPincode.UpdatedAt;
                        existingPincode.UpdatedBy = importPincode.UpdatedBy;
                        _db.Pincodes.Add(existingPincode);
                    }

                    //await _db.SaveChangesAsync();
                }
                await _db.SaveChangesAsync();

                return Ok(importPincodes.Count + " pincodes added");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("drivers")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Drivers()
        {
            try
            {
                var importDrivers = await _importService.GetDrivers();
                if (importDrivers == null)
                {
                    return NotFound();
                }

                var drivers = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.DRIVER).ToList();
                var role = _db.Roles.Where(p => p.Name == RoleNames.DRIVER).FirstOrDefault();
                if (role == null)
                {
                    return BadRequest($"Driver Role is not set.");
                }

                foreach (var importDriver in importDrivers)
                {
                    var existingDriver = drivers.Where(p => p.UserName == importDriver.UserName).FirstOrDefault();
                    if (existingDriver != null)
                    {
                        //existingDriver
                        //_db.Entry(existingDriver).State = EntityState.Modified;
                    }
                    else
                    {
                        existingDriver = new User()
                        {
                            Password = importDriver.Password,
                            UserName = importDriver.UserName,
                            TenantId = importDriver.TenantId,
                            BranchId = importDriver.BranchId,
                            CreationType = CreationType.USER,
                            Type = UserType.DRIVER,
                            OtpValidated = true,
                            CreatedAt = importDriver.CreatedAt,
                            CreatedBy = importDriver.CreatedBy,
                            IsDeleted = importDriver.IsDeleted,
                            LastLogin = importDriver.LastLogin,
                            UpdatedAt = importDriver.UpdatedAt,
                            UpdatedBy = importDriver.UpdatedBy,
                            Roles = new List<UserRole>(),
                            Profile = new UserProfile()
                            {
                                UpdatedBy = importDriver.Profile.UpdatedBy,
                                UpdatedAt = importDriver.Profile.UpdatedAt,
                                AgreedTerms = true,
                                Code = importDriver.Profile.Code,
                                CreatedAt = importDriver.Profile.CreatedAt,
                                CreatedBy = importDriver.Profile.CreatedBy,
                                DeviceId = importDriver.Profile.DeviceId,
                                Email = importDriver.Profile.Email,
                                FirstName = importDriver.Profile.FirstName,
                                LastName = importDriver.Profile.LastName,
                                IsDeleted = importDriver.Profile.IsDeleted,
                                Mobile = importDriver.Profile.Mobile,
                                OffDay = importDriver.Profile.OffDay,
                                PhotoUrl = importDriver.Profile.PhotoUrl,
                                Rating = 5,
                                Source = Shared.Source.DRIVER_APP
                            }
                        };
                        existingDriver.Roles.Add(new UserRole(role.Id));
                        _db.Users.Add(existingDriver);
                    }

                    //await _db.SaveChangesAsync();
                }
                await _db.SaveChangesAsync();

                return Ok(importDrivers.Count + " drivers added");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("vehicles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Vehicles()
        {
            List<string> errors = new List<string>();
            try
            {
                var importVehicles = await _importService.GetVehicles();
                if (importVehicles == null)
                {
                    return NotFound();
                }

                var importDrivers = await _importService.GetDrivers();
                var importDistributors = await _importService.GetDistributors();

                var vehicles = _db.Vehicles.ToList();
                var drivers = _db.Users.Where(p => p.Type == UserType.DRIVER).ToList();
                var distributors = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Distributor).ToList();

                foreach (var importVehicle in importVehicles)
                {
                    var existing = vehicles.Where(p => p.RegNo == importVehicle.RegNo).FirstOrDefault();
                    if (existing != null)
                    {
                        //existingDriver
                        //_db.Entry(existingDriver).State = EntityState.Modified;
                    }
                    else
                    {
                        var importDistributor = importDistributors.Where(p => p.Id == importVehicle.DistributorId).FirstOrDefault();
                        if (importDistributor == null)
                        {
                            continue;
                        }
                        var distributor = distributors.Where(p => p.MobileNumber == importDistributor.Profile.Mobile).FirstOrDefault();
                        if (distributor == null)
                        {
                            errors.Add(importDistributor.Id + " , " + importDistributor.Profile.Mobile + " distributor not found for vehicle " + importVehicle.RegNo);
                            continue;
                        }

                        existing = new Vehicle()
                        {
                            TenantId = distributor.TenantId,
                            BranchId = distributor.BranchId,
                            BusinessEntityId = distributor.Id,
                            CreatedAt = importVehicle.CreatedAt,
                            CreatedBy = importVehicle.CreatedBy,
                            IsActive = importVehicle.IsActive,
                            IsDeleted = importVehicle.IsDeleted,
                            OriginLat = importVehicle.OriginLat,
                            OriginLng = importVehicle.originLng,
                            RegNo = importVehicle.RegNo,
                            State = VehicleState.OutFromWork,
                            DestinationLat = importVehicle.DestinationLat,
                            DestinationLng = importVehicle.DestinationLng,
                            UpdatedAt = importVehicle.UpdatedAt,
                            UpdatedBy = importVehicle.UpdatedBy
                        };
                        if (importVehicle.DriverId != null)
                        {
                            var importDriver = importDrivers.Where(p => p.Id == importVehicle.DriverId).FirstOrDefault();
                            if (importDriver != null)
                            {
                                var driver = drivers.Where(p => p.UserName == importDriver.UserName).FirstOrDefault();
                                if (driver != null)
                                {
                                    driver.BranchId = existing.BranchId;
                                    _db.Entry(driver).State = EntityState.Modified;

                                    existing.DriverId = driver.Id;
                                }
                                else
                                {

                                }
                            }
                        }
                        _db.Vehicles.Add(existing);
                    }

                    //await _db.SaveChangesAsync();
                }

                if (errors.Count == 0)
                {
                    await _db.SaveChangesAsync();
                    return Ok(importVehicles.Count + " vehicles added");
                }
                else
                {
                    return BadRequest(string.Join(',', errors));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("customers")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Customers()
        {
            List<string> errors = new List<string>();
            try
            {
                var importCustomers = await _importService.GetCustomers();
                if (importCustomers == null)
                {
                    return NotFound();
                }

                var importDistributors = await _importService.GetDistributors();
                var distributors = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Distributor).ToList();

                var customers = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.CUSTOMER).ToList();
                var role = _db.Roles.Where(p => p.Name == RoleNames.CUSTOMER).FirstOrDefault();
                if (role == null)
                {
                    return BadRequest($"Customer Role is not set.");
                }

                var pincodes = _db.Pincodes.Include(p => p.Branch).ToList();

                foreach (var importCustomer in importCustomers)
                {
                    var existing = customers.Where(p => p.UserName == importCustomer.UserName).FirstOrDefault();
                    if (existing != null)
                    {
                        //existingDriver
                        //_db.Entry(existingDriver).State = EntityState.Modified;
                    }
                    else
                    {
                        existing = new User()
                        {
                            Password = importCustomer.Password,
                            UserName = importCustomer.UserName,
                            TenantId = importCustomer.TenantId,
                            BranchId = importCustomer.BranchId,
                            CreationType = CreationType.USER,
                            Type = UserType.CUSTOMER,
                            OtpValidated = importCustomer.OtpValidated,
                            OtpValidatedAt = importCustomer.OtpValidatedAt,
                            OtpValidatedBy = importCustomer.OtpValidatedBy,
                            CreatedAt = importCustomer.CreatedAt,
                            CreatedBy = importCustomer.CreatedBy,
                            IsDeleted = importCustomer.IsDeleted,
                            LastLogin = importCustomer.LastLogin,
                            UpdatedAt = importCustomer.UpdatedAt,
                            UpdatedBy = importCustomer.UpdatedBy,
                            Roles = new List<UserRole>(),
                            Addresses = new List<UserAddress>(),
                            Profile = new UserProfile()
                            {
                                MyReferralCode = importCustomer.Profile.MyReferralCode,
                                Gender = Gender.NotSpecified,
                                ReferralCode = importCustomer.Profile.ReferralCode,
                                SendNotifications = true,
                                UpdatedBy = importCustomer.Profile.UpdatedBy,
                                UpdatedAt = importCustomer.Profile.UpdatedAt,
                                AgreedTerms = importCustomer.Profile.AgreedTerms,
                                Code = importCustomer.Profile.Code,
                                CreatedAt = importCustomer.Profile.CreatedAt,
                                CreatedBy = importCustomer.Profile.CreatedBy,
                                DeviceId = importCustomer.Profile.DeviceId,
                                Email = importCustomer.Profile.Email,
                                FirstName = importCustomer.Profile.FirstName,
                                LastName = importCustomer.Profile.LastName,
                                IsDeleted = importCustomer.Profile.IsDeleted,
                                Mobile = importCustomer.Profile.Mobile,
                                //OffDay = importCustomer.Profile.OffDay,
                                PhotoUrl = importCustomer.Profile.PhotoUrl,
                                Rating = 5,
                                Source = importCustomer.Profile.Source
                            }
                        };
                        existing.Roles.Add(new UserRole(role.Id));

                        /*
                        if (importCustomer.Profile.ReferredByUser != null)
                        {
                            var referedUser = customers.Where(p => p.Type == UserType.CUSTOMER && p.UserName == importCustomer.Profile.ReferredByUser.UserName).FirstOrDefault();
                            if (referedUser != null)
                            {

                                existing.Profile.ReferredByUserId = referedUser.Id;
                            }
                            else
                            {

                            }
                        }
                        */

                        if (importCustomer.DistributorId != null)
                        {
                            var importDistributor = importDistributors.Where(p => p.Id == importCustomer.DistributorId).FirstOrDefault();
                            if (importDistributor != null)
                            {
                                var distributor = distributors.Where(p => p.MobileNumber == importDistributor.Profile.Mobile).FirstOrDefault();
                                if (distributor != null)
                                {
                                    existing.BusinessEntityId = distributor.Id;
                                }
                                else
                                {
                                    errors.Add(importDistributor.Id + ", " + importDistributor.UserName + " not found for customer " + existing.UserName);
                                }
                            }
                            
                        }

                        foreach(var importAddr in importCustomer.Addresses) 
                        {
                            var pincode = pincodes.Where(p => p.Code == importAddr.PinCode.ToString()).FirstOrDefault();
                            if (pincode == null)
                            {
                                errors.Add("pincode not found for customer " + importCustomer.UserName);
                                continue;
                            }

                            if (pincode.BranchId == null)
                            {
                                errors.Add("pincode branch not found for pincode " + pincode.Code);
                                continue;
                            }

                            UserAddress address = new UserAddress()
                            {
                                TenantId = 1,
                                BranchId = pincode.BranchId,
                                City = pincode.Branch?.Name,
                                State = importAddr.State,
                                CreatedAt = importAddr.CreatedAt,
                                CreatedBy = importAddr.CreatedBy,
                                IsDeleted = importAddr.IsDeleted,
                                Landmark = importAddr.Landmark,
                                Lat = importAddr.Lat,
                                Lng = importAddr.Lng,
                                Location = importAddr.Location,
                                PinCode = importAddr.PinCode.ToString(),
                                Name = importAddr.BuildingNo,
                                Details = importAddr.BuildingNo + " " + importAddr.StreetNo,
                                PhoneAlternate = importAddr.PhoneAlternate,
                                UpdatedAt = importAddr.UpdatedAt,
                                UpdatedBy = importAddr.UpdatedBy,

                            };
                            existing.Addresses.Add(address);
                        }
                        _db.Users.Add(existing);
                    }

                    //await _db.SaveChangesAsync();
                }

                if (errors.Count == 0)
                {
                    await _db.SaveChangesAsync();
                    return Ok(importCustomers.Count + " customers added");
                }
                else
                {
                    return BadRequest(string.Join(',', errors));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("customers/referrals")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCustomerReferralIds()
        {
            List<string> errors = new List<string>();
            try
            {
                var customers = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.CUSTOMER).ToList();
                foreach(var customer in customers)
                {
                    if (!string.IsNullOrEmpty(customer.Profile.ReferralCode))
                    {
                        var referredCustomer = _db.Profiles.Where(p => p.MyReferralCode == customer.Profile.ReferralCode).FirstOrDefault();
                        if (referredCustomer == null)
                        {
                            referredCustomer = _db.Profiles.Where(p => p.Mobile == customer.Profile.ReferralCode).FirstOrDefault();
                        }
                        if (referredCustomer == null)
                        {
                            errors.Add(customer.Profile.ReferralCode + " referral code user not found for customer " + customer.Id);          
                        }
                        else
                        {
                            customer.Profile.ReferredByUserId = referredCustomer.UserId;
                            _db.Entry(customer).State = EntityState.Modified;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(string.Join(',', errors));

            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("customers/wallet/update")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCustomerWallets()
        {
            List<string> errors = new List<string>();
            try
            {
                List<WalletUpdate> walletUpdates = new List<WalletUpdate>();
                var customers = _db.Users.Include(p => p.Profile).Where(p => p.Type == UserType.CUSTOMER).ToList();
                foreach (var customer in customers)
                {
                    WalletUpdate walletUpdate = new WalletUpdate()
                    {
                        MobileNumber = customer.UserName,
                        NewUniqueId = customer.Id,
                        TenantUID = "EasyGasIndia",
                        WalletType = WalletType.ENDUSER
                    };
                    walletUpdates.Add(walletUpdate);
                }

                bool walletUpdateResponse = await _importService.UpdateWallets(walletUpdates);

                return Ok(walletUpdateResponse);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("distributors/wallet/update")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateDistributorWallets()
        {
            List<string> errors = new List<string>();
            try
            {
                List<WalletUpdate> walletUpdates = new List<WalletUpdate>();
                var distributors = _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Distributor).ToList();
                foreach (var distributor in distributors)
                {
                    WalletUpdate walletUpdate = new WalletUpdate()
                    {
                        MobileNumber = distributor.MobileNumber,
                        NewUniqueId = distributor.Id,
                        TenantUID = "EasyGasIndia",
                        WalletType = WalletType.DISTRIBUTOR
                    };
                    walletUpdates.Add(walletUpdate);
                }

                bool walletUpdateResponse = await _importService.UpdateWallets(walletUpdates);

                return Ok(walletUpdateResponse);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private BusinessEntity CreateBusinessEntityFromDistributorModel(ImportUser model)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            var businessEntity = new BusinessEntity()
            {
                Type = BusinessEntityType.Distributor,
                TenantId = 1,
                BranchId = (int)model.BranchId,
                Code = string.IsNullOrEmpty(model.Profile.Code) ? "" : model.Profile.Code,
                Name = model.Profile.FirstName + " " + model.Profile.LastName,
                MobileNumber = model.Profile.Mobile,
                Email = model.Profile.Email,
                //WorkingStartDay = model.WorkingStartDay,
                //WorkingEndDay = model.WorkingEndDay,
                //WorkingStartTime = model.WorkingStartTime,
                //WorkingEndTime = model.WorkingEndTime,
                Location = model.Addresses.FirstOrDefault()?.Location,
                Lat = (double)model.Addresses.FirstOrDefault()?.Lat,
                Lng = (double)model.Addresses.FirstOrDefault()?.Lng,
                GeoLocation = geometryFactory.CreatePoint(new Coordinate((double)model.Addresses.FirstOrDefault()?.Lng, (double)model.Addresses.FirstOrDefault()?.Lat)),
                Landmark = model.Addresses.FirstOrDefault()?.Landmark,
                Details = model.Addresses.FirstOrDefault()?.BuildingNo + ", " + model.Addresses.FirstOrDefault()?.StreetNo,
                PinCode = model.Addresses.FirstOrDefault()?.PinCode.ToString(),
                State = model.Addresses.FirstOrDefault()?.State,
                //ParentBusinessEntityId = model.ParentBusinessEntityId,
                Rating = 5,

                GSTN = model.Profile.GSTN,
                PAN = model.Profile.PAN,
                PaymentNumber = model.Profile.PaymentNumber,
                UPIQRCodeImageUrl = model.Profile.UPIQRCodePath,
                IsDeleted = false,
                
                IsActive = true,
                Timings = new List<BusinessEntityTiming>()
            };

            for (var i = 0; i < 7; i++)
            {
                businessEntity.Timings.Add(new BusinessEntityTiming()
                {
                    Day = (DayOfWeek)i,
                    IsActive = true
                });
            }
            return businessEntity;
        }

        private User CreateDistributorUserFromModel(ImportUser model)
        {
            var user = new User()
            {
                Password = model.Profile.Mobile,
                UserName = model.Profile.Mobile,
                TenantId = model.TenantId,
                BranchId = model.BranchId,
                CreationType = CreationType.USER,
                Type = UserType.DISTRIBUTOR,
                OtpValidated = false,
                Roles = new List<UserRole>() 
            };
            return user;
        }

        private UserProfile CreateDistributorProfileFromModel(ImportUser model)
        {
            var profile = new UserProfile()
            {
                Email = model.Profile.Email,
                Mobile = model.Profile.Mobile,
                FirstName = model.Profile.FirstName,
                LastName = model.Profile.LastName,
                Gender = Gender.NotSpecified,
                Source = model.Profile.Source,
                Code = model.Profile.Code,
                AgreedTerms = true,
                
            };

            return profile;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Profiles.API.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Profiles.API.Services;
using Profiles.API.Models;
using EasyGas.Shared.Formatters;
using Profiles.API.ViewModels.Distributor;
using Microsoft.AspNetCore.Mvc;

namespace EasyGas.Services.Profiles.Queries
{
    public class ProfileQueries : IProfileQueries
    {
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _ctx;
        private IFeedbackQueries _feedbackQueries;
        private readonly ITenantQueries _tenantQueries;
        private readonly IOrderService _orderService;

        private List<UserType> staffUserTypes = new List<UserType>()
            {
                UserType.DRIVER, UserType.SECURITY, UserType.MARSHAL, UserType.PICKUP_DRIVER
            };

        public ProfileQueries(IOptions<ApiSettings> apiSettings, ProfilesDbContext ctx, 
            IFeedbackQueries feedbackQueries, ITenantQueries tenantQueries, IOrderService orderService)
        {
            _apiSettings = apiSettings;
            _ctx = ctx;
            _feedbackQueries = feedbackQueries;
            _tenantQueries = tenantQueries;
            _orderService = orderService;
        }

        public async Task<IEnumerable<UserDetailsModel>> GetCustomersBySearch(string term, int from, int size, int? tenantId, int? branchId, int? businessEntityId = null)
        {
            List<UserDetailsModel> customers = new List<UserDetailsModel>();
            var usersQuery = _ctx.Users
                .Include(p => p.Profile.ReferredByUser)
                .Include(t => t.Profile)
                .Include(t => t.Addresses)
                .Include(p => p.BusinessEntity)
                .Where(t => t.Type == UserType.CUSTOMER && ((t.Profile.FirstName ?? string.Empty).ToLower().Contains((term ?? string.Empty).ToLower()) || (t.Profile.LastName ?? string.Empty).ToLower().Contains((term ?? string.Empty).ToLower()) || (t.Profile.Mobile ?? string.Empty).ToLower().Contains((term ?? string.Empty).ToLower())))
                .AsQueryable();

            if (tenantId != null)
            {
                usersQuery = usersQuery.Where(p => p.TenantId == tenantId);
            }
            if (branchId != null)
            {
                usersQuery = usersQuery.Where(p => p.BranchId == branchId);
            }
            if (businessEntityId != null)
            {
                usersQuery = usersQuery.Where(p => p.BusinessEntityId == businessEntityId);
            }
            var users = await usersQuery.Skip(from).Take(size).ToListAsync();
            foreach ( var user in users)
            {
                UserDetailsModel customer = MapToUserWithDetailsModel(user);
                customers.Add(customer);
            }
            return customers.AsEnumerable();
        }

        public async Task<IEnumerable<UserAndProfileModel>> GetCustomersList(DateTime from, DateTime to, int? tenantId, int? branchId)
        {
            List<UserAndProfileModel> customers = new List<UserAndProfileModel>();
            try
            {
                var users = _ctx.Users
                    .Include(t => t.Profile)
                    .Include(p => p.Profile.ReferredByUser)
                    .Include(p => p.BusinessEntity)
                    .Include(p => p.BusinessEntity.Branch)
                    .Include(p => p.Branch)
                    //.Include(t => t.Addresses)
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to && t.Type == UserType.CUSTOMER)
                    .ToList();
                if (tenantId != null)
                {
                    users = users.Where(p => p.TenantId == tenantId).ToList();
                }
                if (branchId != null && branchId > 0)
                {
                    var customerIds = await _orderService.GetCustomerIdsOrderedInBranch((int)branchId);
                    if (customerIds != null)
                    {
                        users = users.Where(p => customerIds.Contains(p.Id)).ToList();
                    }
                }

                foreach (var user in users)
                {
                    UserAndProfileModel customer = MapToUserAndProfileModel(user);
                    customers.Add(customer);
                }
                return customers.AsEnumerable();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return customers.AsEnumerable();
        }

        public async Task<List<DistributorCustomerModel>> GetCustomersListOfBusinessEntity(int businessEntityId, int distributerUserId)
        {
            List<DistributorCustomerModel> customers = await _ctx.Users
                    .Include(t => t.Profile)
                    .Include(p => p.Profile.ReferredByUser)
                    //.Include(p => p.BusinessEntity)
                    //.Include(p => p.BusinessEntity.Branch)
                    .Include(t => t.Addresses)
                    .Where(t => t.Type == UserType.CUSTOMER && t.BusinessEntityId == businessEntityId)
                    .Select(p => new DistributorCustomerModel()
                    {
                        CreatedAt = p.CreatedAt,
                        Email = p.Profile.Email,
                        Mobile = p.Profile.Mobile,
                        FirstName = p.Profile.FirstName,
                        LastName = p.Profile.LastName,
                        LastLoginAt = p.LastLogin,
                        IsReferredByDistributor = p.Profile.ReferredByUserId == distributerUserId,
                        Source = p.Profile.Source
                    })
                    .ToListAsync();

                return customers;
        }

        public async Task<IEnumerable<AddressModel>> GetUserAddressList(int userId)
        {
            List<AddressModel> addresses = new List<AddressModel>();
            var userAddresses = await _ctx.Addresses.Where(p => p.UserId == userId).ToListAsync();
            if (userAddresses.Count > 0)
            {
                    foreach (var address in userAddresses)
                    {
                        AddressModel userAddress = MapToAddressModel(address);
                        addresses.Add(userAddress);
                    }
            }
            return addresses.AsEnumerable();
        }

        public async Task<AddressModel> GetUserAddress(int userAddressId, int userId)
        {
            List<AddressModel> addresses = new List<AddressModel>();
            var userAddress = await _ctx.Addresses.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == userAddressId);
            if (userAddress != null)
            {
                AddressModel userAddressModel = MapToAddressModel(userAddress);
                return userAddressModel;
            }
            return null;
        }

        public async Task<UserDetailsModel> GetDetailsByUserId(int userId)
        {
            var user = await _ctx.Users
                .Include(t => t.Profile)
                .Include(p => p.Profile.ReferredByUser)
                .Include(t => t.Addresses)
                .Where(p => p.Id == userId)
                .SingleOrDefaultAsync();

            if(user != null)
            {
                return MapToUserWithDetailsModel(user);
            }
            return null;
        }

        public async Task<UserAndProfileModel> GetByUserId(int userid)
        {
            UserAndProfileModel profile = new UserAndProfileModel();
            User user = await _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Profile.ReferredByUser)
                .SingleOrDefaultAsync(p => p.Id == userid);
            if (user != null)
            {
                profile = MapToUserAndProfileModel(user, true);
            }
            return profile;
        }

        public async Task<UserAndProfileModel> GetByCode(string code)
        {
            UserAndProfileModel profile = new UserAndProfileModel();
            User user = await _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Profile.ReferredByUser)
                .SingleOrDefaultAsync(p => p.Profile.MyReferralCode == code);
            if (user != null)
            {
                profile = MapToUserAndProfileModel(user, true);
            }
            return null;
        }

        public async Task<CustomerProfileModel> GetCustomerProfileByUserId(int userid)
        {
            CustomerProfileModel profile = new CustomerProfileModel();
            User user = await _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Profile.ReferredByUser)
                .Include(p => p.BusinessEntity)
                .Include(p => p.BusinessEntity.Branch)
                .SingleOrDefaultAsync(p => p.Id == userid);

            if (user != null)
            {
                profile = MapToCustomerProfileModel(user, true);
            }
            return profile;
        }

        public async Task<DriverProfileModel> GetDriverProfileByUserId(int userid)
        {
            DriverProfileModel driverProfile = new DriverProfileModel();
            var driver = await _ctx.Users.Include(p => p.Profile).Include(p => p.BusinessEntity).Include(p => p.Addresses).Where(p => p.Id == userid).FirstOrDefaultAsync();
            if (driver != null)
            {
                driverProfile.CityId = driver.BranchId;
                driverProfile.Email = driver.Profile.Email;
                driverProfile.FirstName = driver.Profile.FirstName;
                driverProfile.LastName = driver.Profile.LastName;
                driverProfile.Mobile = driver.Profile.Mobile;
                driverProfile.OffDay = driver.Profile.OffDay;
                driverProfile.PhotoUrl = GetPhotoUrl(driver.Profile.PhotoUrl);
                driverProfile.AverageRating = driver.Profile.Rating;
                
                var veh = _ctx.Vehicles.Include(p => p.BusinessEntity).Where(p => p.DriverId == userid).FirstOrDefault();
                if (veh != null)
                {
                    driverProfile.VehicleId = veh.Id;
                    driverProfile.VehicleName = veh.RegNo;
                    driverProfile.VehicleOriginLat = veh.OriginLat;
                    driverProfile.VehicleOriginLng = veh.OriginLng;
                    driverProfile.VehicleDestinationLat = veh.DestinationLat;
                    driverProfile.VehicleDestinationLng = veh.DestinationLng;

                    if (veh.BusinessEntity != null)
                    {
                        driverProfile.BusinessEntityName = veh.BusinessEntity.Name;
                        driverProfile.BusinessEntityId = veh.BusinessEntityId;
                    }
                }

                if (driver.Addresses != null)
                {
                    if (driver.Addresses.Count > 0)
                    {
                        driverProfile.Address = MapToAddressModel(driver.Addresses.First());
                    }
                }

                return driverProfile;
            }
            return null;
        }

        public async Task<UserProfile> GetByEmail(string value)
        {
            return await _ctx.Profiles.FirstOrDefaultAsync(p => p.Email == value);
        }
        public async Task<UserProfile> GetByMobile(string value)
        {
            return await _ctx.Profiles.FirstOrDefaultAsync(p => p.Mobile == value);
        }

        public async Task<UserAndProfileModel> GetUserAndProfileByMobile(string value, UserType userType, int tenantId)
        {
            UserAndProfileModel model = new UserAndProfileModel();
            User user = await _ctx.Users.Include(p => p.Profile).Include(p => p.Profile.ReferredByUser).FirstOrDefaultAsync(p => p.UserName == value && p.Type == userType && p.TenantId == tenantId);
            if (user != null)
            {
                model = MapToUserAndProfileModel(user, true);
            }
            return model;
        }

        public int GetCountByCreationDate(DateTime? fromDate, DateTime? toDate, UserType? type, int? tenantId, int? branchId)
        {
            var customers = _ctx.Users.AsQueryable();
            if (fromDate != null)
            {
                customers = customers.Where(p => p.CreatedAt.Date >= fromDate);
            }
            if (toDate!= null)
            {
                customers = customers.Where(p => p.CreatedAt.Date <= toDate);
            }
            if (type!= null)
            {
                customers = customers.Where(p => p.Type == type);
            }
            if (tenantId != null)
            {
                customers = customers.Where(p => p.TenantId == tenantId);
            }
            if (branchId != null)
            {
                customers = customers.Where(p => p.BranchId == branchId);
            }
            return customers.Count();
        }

        public async Task<List<DriverModel>> GetDriverList(int? tenantId, int? branchId, int? businessEntityId)
        {
            List<DriverModel> driverList = new List<DriverModel>();
            try
            {
                var drivers = _ctx.Users
                    .Include(p => p.Profile)
                    .Include(p => p.Profile.ReferredByUser)
                    .Where(p => p.Type == UserType.DRIVER);
                if (tenantId != null)
                {
                    drivers = drivers.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    drivers = drivers.Where(p => p.BranchId == branchId);
                }
                if (businessEntityId != null)
                {
                    drivers = drivers.Where(p => p.BusinessEntityId == businessEntityId);
                }
                var driversList = await drivers.ToListAsync();
                foreach (var driver in driversList)
                {
                    DriverModel model = new DriverModel();
                    model.UserProfile = MapToUserAndProfileModel(driver, false);
                    driverList.Add(model);
                }
            }
            catch(Exception ex)
            {

            }
            return driverList;
        }

        public async Task<List<SelectListItem>> GetDriverSelectList(int? tenantId, int? branchId, int? businessEntityId)
        {
                var drivers = _ctx.Users.Include(p => p.Profile).Where(p => p.Type == UserType.DRIVER);
                if (tenantId != null)
                {
                    drivers = drivers.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    drivers = drivers.Where(p => p.BranchId == branchId);
                }
                if (businessEntityId != null)
                {
                    drivers = drivers.Where(p => p.BusinessEntityId == businessEntityId);
                }
            var driversList = await drivers.Select(p => new SelectListItem() { 
                Text = p.Profile.GetFullName(),
                Value = p.Id.ToString()
            }).ToListAsync();

                return driversList;

        }

        public async Task<List<StaffModel>> GetStaffList(int? tenantId, int? branchId, int? businessEntityId)
        {
            List<StaffModel> staffList = new List<StaffModel>();
            try
            {
                var staffs = _ctx.Users
                    .Include(p => p.Profile)
                    .Include(p => p.Profile.ReferredByUser)
                    .Where(p => staffUserTypes.Contains(p.Type));
                if (tenantId != null)
                {
                    staffs = staffs.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    staffs = staffs.Where(p => p.BranchId == branchId);
                }
                if (businessEntityId != null)
                {
                    staffs = staffs.Where(p => p.BusinessEntityId == businessEntityId);
                }
                var staffsList = await staffs.ToListAsync();
                foreach (var staff in staffsList)
                {
                    StaffModel model = StaffModel.FromUser(staff);
                    staffList.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return staffList;
        }

        public async Task<StaffModel> GetStaffDetail(int staffId)
        {
            StaffModel staff = new StaffModel();
            User user = await _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Profile.ReferredByUser)
                .SingleOrDefaultAsync(p => p.Id == staffId);
            if (user != null)
            {
                staff = StaffModel.FromUser(user);
            }
            return staff;
        }

        public async Task<List<DistributorModel>> GetDistributorList(int? tenantId, int? branchId)
        {
            List<DistributorModel> distributorList = new List<DistributorModel>();
            try
            {
                var distributors = _ctx.Users.Include(p => p.Profile).Include(p => p.Profile.ReferredByUser).Include(t => t.Addresses).Where(p => p.Type == UserType.DISTRIBUTOR);
                if (branchId != null)
                {
                    distributors = distributors.Where(p => p.BranchId == branchId);
                }
                if (tenantId != null)
                {
                    distributors = distributors.Where(p => p.TenantId == tenantId);
                }
                var list = await distributors.ToListAsync();
                foreach (var distributor in list)
                {
                    DistributorModel model = MapToDistributorModel(distributor);
                    distributorList.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return distributorList;
        }

        public async Task<CommandResult> GetAppConfig(int userId, UserType type)
        {
            DriverAppMasterVM driverAppModel = new DriverAppMasterVM()
            {
                CallCenterNumber = "",
                OrderCancelComboList = new List<FeedbackModel>(),
                OrderFeedbackComboList = new List<FeedbackModel>()
            };
            try
            {
                var user = _ctx.Users.Where(p => p.Id == userId && p.Type == type).FirstOrDefault();
                if (user != null)
                {
                    driverAppModel.CallCenterNumber = _ctx.Branches.Where(p => p.Id == user.BranchId && p.TenantId == user.TenantId).Select(t => t.CallCenterNumber).FirstOrDefault();
                    if (type == UserType.DRIVER)
                    {
                        var feedbackComboList = await _feedbackQueries.GetFeedbackList(user.BranchId, user.TenantId, FeedbackType.DRIVER_FEEDBACK);
                        driverAppModel.OrderFeedbackComboList = feedbackComboList.ToList();
                        var cancelComboList = await _feedbackQueries.GetFeedbackList(user.BranchId, user.TenantId, FeedbackType.DRIVER_CANCEL);
                        driverAppModel.OrderCancelComboList = cancelComboList.ToList();
                        driverAppModel.PaymentWebViewUrl = _apiSettings.Value.DriverPaymentWebViewUrl;
                    }
                    else if (type == UserType.CUSTOMER)
                    {
                        var feedbackComboList = await _feedbackQueries.GetFeedbackList(user.BranchId, user.TenantId, FeedbackType.CUSTOMER_FEEDBACK);
                        driverAppModel.OrderFeedbackComboList = feedbackComboList.ToList();
                        var cancelComboList = await _feedbackQueries.GetFeedbackList(user.BranchId, user.TenantId, FeedbackType.CUSTOMER_CANCEL);
                        driverAppModel.OrderCancelComboList = cancelComboList.ToList();
                        driverAppModel.PaymentWebViewUrl = _apiSettings.Value.CustomerPaymentWebViewUrl;
                    }
                }
                else
                {
                    IEnumerable<string> validationErrors = new List<string>() { "User is invalid" };
                    return CommandResult.FromValidationErrors(validationErrors);
                }
            }
            catch(Exception e)
            {
                List<string> validationErrors = new List<string>() { "Some internal error has occured." };
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
            return new CommandResult(System.Net.HttpStatusCode.OK, driverAppModel);
        }

        public async Task<CommandResult> GetDriverAppConfig(int userId)
        {
            DriverAppMasterVM driverAppModel = new DriverAppMasterVM()
            {
                CallCenterNumber = "",
                OrderCancelComboList = new List<FeedbackModel>(),
                OrderFeedbackComboList = new List<FeedbackModel>()
            };

            var user = _ctx.Users.Where(p => p.Id == userId && p.Type == UserType.DRIVER).FirstOrDefault();
            if (user == null)
            {
                IEnumerable<string> validationErrors = new List<string>() { "User is invalid" };
                return CommandResult.FromValidationErrors(validationErrors);
            }

            int tenantId = 1; //TODO remove hardcode

            driverAppModel.CallCenterNumber = _ctx.Branches.Where(p => p.Id == user.BranchId && p.TenantId == user.TenantId).Select(t => t.CallCenterNumber).FirstOrDefault();

            var feedbackComboList = await _feedbackQueries.GetFeedbackList(null, tenantId, FeedbackType.DRIVER_FEEDBACK);
            driverAppModel.OrderFeedbackComboList = feedbackComboList.ToList();
            var cancelComboList = await _feedbackQueries.GetFeedbackList(null, tenantId, FeedbackType.DRIVER_CANCEL);
            driverAppModel.OrderCancelComboList = cancelComboList.ToList();

            return new CommandResult(System.Net.HttpStatusCode.OK, driverAppModel);
        }

        public async Task<CustomerAppConfig> GetCustomerAppConfig(int? branchId)
        {
            int tenantId = 1; //TODO remove hardcode
            IEnumerable<Branch> branches = await _tenantQueries.GetAllBranchesAsync(tenantId);
            branches = branches.Where(p => p.IsActive).AsEnumerable();
            List<BranchModel> branchList = new List<BranchModel>();
            foreach (var branch in branches)
            {
                branchList.Add(new BranchModel() 
                { 
                    Id = branch.Id,
                    Name = branch.Name,
                    CallCenterNumber = branch.CallCenterNumber,
                    PictureUrl = branch.PictureUrl,
                    Lat = branch.Lat,
                    Lng = branch.Lng
                });
            }

            string imageBaseUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerAppImageContainer + "/";

            List<string> appImages = new();
            if (branchId != null)
            {
                appImages = await _ctx.AppImages
                .Where(p => p.Type == AppImageType.CustomerAppHomepage && p.TenantId == tenantId 
                && p.BranchId == branchId)
                .OrderBy(p => p.Position)
                .Select(p => imageBaseUrl + p.FileName)
                .ToListAsync();
            }
            else
            {
                appImages = await _ctx.AppImages
                .Where(p => p.Type == AppImageType.CustomerAppHomepage && p.TenantId == tenantId
                && p.BranchId == null)
                .OrderBy(p => p.Position)
                .Select(p => imageBaseUrl + p.FileName)
                .ToListAsync();
            }

            var appconfig = new CustomerAppConfig()
            {
                CityList = branchList,
                HomepageSlideshowImageList = appImages
                //{
                 //   "https://totaloilrgdiag.blob.core.windows.net/customer-app/CAROUSEL-1.jpg",
                //    "https://totaloilrgdiag.blob.core.windows.net/customer-app/CAROUSEL-2.jpg",
                //   "https://totaloilrgdiag.blob.core.windows.net/customer-app/CAROUSEL-3.jpg",
                //    "https://totaloilrgdiag.blob.core.windows.net/customer-app/CAROUSEL-4.jpg"
                //}
            };

            var feedbackComboMasterList = await _feedbackQueries.GetFeedbackList(null, tenantId, null);

            appconfig.OrderFeedbackComboList = feedbackComboMasterList
                .Where(p => p.FeedbackType == FeedbackType.CUSTOMER_FEEDBACK)
                .Select(p => p.Remarks)
                .ToList();

            appconfig.OrderCancelComboList = feedbackComboMasterList
                .Where(p => p.FeedbackType == FeedbackType.CUSTOMER_CANCEL)
                .Select(p => p.Remarks)
                .ToList();

            appconfig.SurrenderFeedbackComboList = feedbackComboMasterList
                .Where(p => p.FeedbackType == FeedbackType.CUSTOMER_SURRENDER_FEEDBACK)
                .Select(p => p.Remarks)
                .ToList();

            return appconfig;
        }

        public async Task<List<AppImage>> GetAppImages(int? tenantId, int? branchId)
        {
            string imageBaseUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerAppImageContainer + "/";
            var appImagesQuery = _ctx.AppImages
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Position)
                //.Select(p => imageBaseUrl + p.FileName)
                .AsQueryable();

            if (tenantId != null)
            {
                appImagesQuery = appImagesQuery.Where(p => p.TenantId == tenantId);
            }

            if (branchId != null)
            {
                appImagesQuery = appImagesQuery.Where(p => p.BranchId == branchId);
            }

            var appImages = await appImagesQuery.ToListAsync();

            foreach (var appImage in appImages)
            {
                appImage.FileName = imageBaseUrl + appImage.FileName;
            }
            return appImages;
        }

        public Task<int> GetInstalledNotRegisteredCustomerCount(int tenantId, int? branchId)
        {
            int installedNotRegisteredCount = 0;
            try
            {
                var installedDevicesList = _ctx.UserDevices.Where(p => p.UserId == null && p.FirebaseDeviceId != null).Select(p => p.FirebaseDeviceId).Distinct().ToList();
                int installedRegisteredCount = _ctx.Profiles.Where(p => installedDevicesList.Contains(p.DeviceId)).Count();
                installedNotRegisteredCount = installedDevicesList.Count - installedRegisteredCount;
            }
            catch (Exception e)
            {
                
            }
            return Task.FromResult(installedNotRegisteredCount);
        }

        public async Task<DeliverySlotModel> GetDeliverySlot(int id)
        {
            var slot = await _ctx.DeliverySlots.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (slot == null)
            {
                return null;
            }

            var slotModel = new DeliverySlotModel()
            {
                Id = slot.Id,
                UID = slot.UID,
                Description = slot.Description,
                From = slot.From,
                Price = slot.Price,
                FromSec = slot.FromSec,
                Name = slot.Name,
                TenantId = slot.TenantId,
                BranchId = slot.BranchId,
                //BranchName = slot.Branch.Name,
                To = slot.To,
                ToSec = slot.ToSec,
                MaxThreshold = slot.MaxThreshold,
                Type = slot.Type,
                IsActive = slot.IsActive
            };

            return slotModel;
        }

        public async Task<IEnumerable<DeliverySlotModel>> GetDeliverySlotList(int branchId)
        {
            var slots = await _ctx.DeliverySlots.Include(p => p.Branch).Where(p => p.BranchId == branchId && p.IsActive)
                    .Select(p => new DeliverySlotModel()
                    {
                        Id = p.Id,
                        UID = p.UID,
                        Description = p.Description,
                        From = p.From,
                        Price = p.Price,
                        FromSec = p.FromSec,
                        Name = p.Name,
                        TenantId = p.TenantId,
                        BranchId = p.BranchId,
                        BranchName = p.Branch.Name,
                        To = p.To,
                        ToSec = p.ToSec,
                        MaxThreshold = p.MaxThreshold,
                        Type = p.Type,
                        IsActive = p.IsActive
                    }).ToListAsync();
            return slots;
        }

        //complaints
        public async Task<List<CustomerComplaintModel>> GetCustomerTickets(int? userId = null, int? tenantId = null, int? branchId = null)
        {
            var complaints = _ctx.Complaints.Where(p => !p.IsDeleted).AsQueryable();
            if (userId != null)
            {
                complaints = complaints.Where(p => p.UserId == userId);
            }
            if (tenantId != null)
            {
                complaints = complaints.Where(p => p.TenantId == tenantId);
            }
            if (branchId != null)
            {
                complaints = complaints.Where(p => p.BranchId == branchId);
            }

            var complaintList = await complaints.OrderByDescending(p => p.CreatedAt).ToListAsync();

            var storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerComplaintsImageContainer;
            List<CustomerComplaintModel> complaintModelList = new List<CustomerComplaintModel>();
            foreach (var complaint in complaintList)
            {
                complaintModelList.Add(CustomerComplaintModel.FromComplaint(complaint, storageUrl));
            }
            return complaintModelList;
        }

        public async Task<PvtWebDashboardVM> GetCrmTicketInfoForAdminDashboard(int? tenantId = null)
        {
            var today = DateMgr.GetCurrentIndiaTime().Date;
            var complaints = _ctx.Complaints.Where(p => !p.IsDeleted).AsQueryable();

            if (tenantId != null)
            {
                complaints = complaints.Where(p => p.TenantId == tenantId);
            }

            var ticketList = await complaints.OrderByDescending(p => p.CreatedAt).ToListAsync();
            PvtWebDashboardVM response = new PvtWebDashboardVM()
            {
                CrmTicketsClosed = ticketList.Where(p => p.Status == ComplaintStatus.Closed).Count(),
                CrmTicketsClosedToday = ticketList.Where(p => p.Status == ComplaintStatus.Closed && p.ClosedAt >= today).Count(),
                CrmTicketsOpen = ticketList.Where(p => p.Status != ComplaintStatus.Closed).Count(),
                CrmTicketsOpenToday = ticketList.Where(p => p.Status != ComplaintStatus.Closed && (p.ReOpenAt >= today || p.CreatedAt >= today)).Count(),
            };

            return response;
        }

        public async Task<IEnumerable<BackendUserProfileModel>> GetBackendUsers(int tenantId, int? branchId)
        {
            List<BackendUserProfileModel> backendUsersList = new List<BackendUserProfileModel>();

            var users = _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Branch)
                .Include(p => p.Roles)
                .ThenInclude(p => p.Role)
                .Where(p => p.Type == UserType.ADMIN && p.TenantId == tenantId)
                .AsQueryable();

            if (branchId != null)
            {
                users = users.Where(p => p.BranchId == branchId);
            }

            var usersList = await users.ToListAsync();

            foreach (var user in usersList)
            {
                BackendUserProfileModel model = BackendUserProfileModel.FromUser(user);
                backendUsersList.Add(model);
            }

            return backendUsersList.AsEnumerable();
        }

        public async Task<BackendUserProfileModel> GetUserByCognitoUsername(string cognitoUsername)
        {
            List<BackendUserProfileModel> backendUsersList = new List<BackendUserProfileModel>();

            var user = await _ctx.Users
                .Include(p => p.Profile)
                .Include(p => p.Branch)
                .Include(p => p.Roles)
                .ThenInclude(p => p.Role)
                .Where(p => p.CognitoUsername == cognitoUsername)
                .FirstOrDefaultAsync();

 
            BackendUserProfileModel model = BackendUserProfileModel.FromUser(user);
            return model;
        }

        private string GetPhotoUrl(string photoUrl)
        {
            if (!string.IsNullOrEmpty(photoUrl))
            {
                return _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobProfileImageContainer + "/" + photoUrl;
            }

            return null;
        }


        private UserAndProfileModel MapToUserAndProfileModel(User user, bool checkIfImageExists = false)
        {

            UserAndProfileModel model = new UserAndProfileModel()
            {
                UserId = user.Id,
                Type = user.Type,
                UserName = user.UserName,
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Email = user.Profile.Email,
                BirthDate = user.Profile.BirthDate,
                Mobile = user.Profile.Mobile,
                Gender = user.Profile.Gender,
                TenantId = user.TenantId,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                AverageRating = user.Profile.Rating,
                PhotoUrl = user.Profile.PhotoUrl,
                GSTN = user.Profile.GSTN,
                PAN = user.Profile.PAN,
                Code = user.Profile.Code,
                ReferralCode = user.Profile.ReferralCode,
                ReferredByUserFullName = user.Profile.ReferredByUser?.UserName,
                MyReferralCode = user.Profile.MyReferralCode,
                CreatedAt = user.Profile.CreatedAt,
                OtpValidated = user.OtpValidated,
                OtpValidatedAt = user.OtpValidatedAt,
                OtpValidatedBy = user.OtpValidatedBy,
                AgreedTerms = user.Profile.AgreedTerms,
                DeviceId = user.Profile.DeviceId,
                OffDay = user.Profile.OffDay,
                IsAmbassador = user.Profile.MyReferralCode != null && user.Profile.MyReferralCode.ToLower().StartsWith(_apiSettings.Value.AmbassadorReferralCodeStartsWith.ToLower()),
                BusinessEntityId = user.BusinessEntityId,
                BusinessEntityName = user.BusinessEntity?.Name,
                BusinessEntityBranchName = user.BusinessEntity?.Branch?.Name
            };

            return model;
        }

        private CustomerProfileModel MapToCustomerProfileModel(User user, bool checkIfImageExists = false)
        {

            CustomerProfileModel model = new CustomerProfileModel()
            {
                UserName = user.UserName,
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Email = user.Profile.Email,
                Mobile = user.Profile.Mobile,
                CityId = user.BranchId,
                AverageRating = user.Profile.Rating,
                PhotoUrl = GetPhotoUrl(user.Profile.PhotoUrl),
                Code = user.Profile.Code,
                ReferralCode = user.Profile.ReferralCode,
                ReferredByUserFullName = user.Profile.ReferredByUser?.UserName,
                MyReferralCode = user.Profile.MyReferralCode,
                CreatedAt = user.Profile.CreatedAt,
                OtpValidated = user.OtpValidated,
                SendNotifications = user.Profile.SendNotifications,
                BusinessEntityId = user.BusinessEntityId,
                BusinessEntityName = user.BusinessEntity?.Name,
                BusinessEntityBranchName = user.BusinessEntity?.Branch?.Name
            };

            return model;
        }

        private AddressModel MapToAddressModel(UserAddress address)
        {
            AddressModel model = new AddressModel()
            {
                TenantId = address.TenantId,
                BranchId = address.BranchId,
                UserAddressId = address.Id,
                Name = address.Name,
                Details = address.Details,
                Landmark = address.Landmark,
                City = address.City,
                State = address.State,
                PinCode = address.PinCode,
                Location = address.Location,
                Lat = address.Lat,
                Lng = address.Lng,
                PhoneAlternate = address.PhoneAlternate
            };
            return model;
        }

        private UserDetailsModel MapToUserWithDetailsModel(User user)
        {
            UserDetailsModel customer = new UserDetailsModel()
            {
                UserAndProfile = MapToUserAndProfileModel(user, false),
                AddressList = new List<AddressModel>()
            };
            if (user.Addresses.Count > 0)
            {
                foreach (var address in user.Addresses)
                {
                    AddressModel userAddress = MapToAddressModel(address);
                    customer.AddressList.Add(userAddress);
                }
            }

            return customer;
        }

        private DistributorModel MapToDistributorModel(User user)
        {
            DistributorModel distributor = new DistributorModel()
            {
                UserProfile = MapToUserAndProfileModel(user, false),
                Address = new AddressModel()
            };
            if (user.Addresses != null)
            {
                if (user.Addresses.Count > 0)
                {
                    foreach (var address in user.Addresses)
                    {
                        AddressModel userAddress = MapToAddressModel(address);
                        distributor.Address = userAddress;
                    }
                }
            }

            return distributor;
        }
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;
using EasyGas.Shared.Models;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Profiles.API.Models;
using Profiles.API.Services;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class ProfileMgr
    {
        private ProfilesDbContext _db;
        private ISmsSender _smsSender;
        private IEmailSender _emailSender;
        private IProfileQueries _profileQueries;
        private IOrderService _orderService;
        private OtpMgr _otpMgr;
        private string _reservedAmbReferralCodeStarting;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ILogger<ProfileMgr> _logger;

        public ProfileMgr(ProfilesDbContext db, IOptions<ApiSettings> apiSettings, ISmsSender smsSender,
            IProfileQueries profileQueries, IEmailSender emailSender, IOrderService orderService,
            OtpMgr otpMgr, ILogger<ProfileMgr> logger)
        {
            _db = db;
            _smsSender = smsSender;
            _profileQueries = profileQueries;
            _otpMgr = otpMgr;
            _orderService = orderService;
            _apiSettings = apiSettings;
            _emailSender = emailSender;
            _reservedAmbReferralCodeStarting = _apiSettings.Value.AmbassadorReferralCodeStartsWith.ToLower();
            _logger = logger;
        }

        public async Task<CommandResult> SetBranch(int userId, int branchId)
        {
            _logger.LogInformation($"ProfileMgr.SetBranch : userId: {userId} branchId: {branchId} ");

            List<string> validationErrors = new List<string>();
            try
            {
                if (userId <= 0 || branchId <= 0)
                {
                    validationErrors.Add("User/City is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                var user = _db.Users.Where(x => x.Id == userId).FirstOrDefault();
                if (user == null)
                {
                    validationErrors.Add("User is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                var branch = _db.Branches.Where(x => x.Id == branchId).FirstOrDefault();
                if (branch == null)
                {
                    validationErrors.Add("Branch is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                user.BranchId = branchId;
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new CommandResult(System.Net.HttpStatusCode.OK, new BranchModel { Id = branchId, Name = branch.Name });
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ProfileMgr.SetBranch Exception : userId: {userId} branchId: {branchId} " + ex.Message);
                validationErrors.Add("Some internal error has occurred " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> SetBranchByPincode(int userId, string pincode)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                if (userId <= 0 || string.IsNullOrEmpty(pincode))
                {
                    validationErrors.Add("User/Pincode is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                var user = _db.Users.Where(x => x.Id == userId).FirstOrDefault();
                if (user == null)
                {
                    validationErrors.Add("User is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                var pincodeModel = _db.Pincodes.Include(x => x.Branch).Where(x => x.Code == pincode).FirstOrDefault();
                if (pincodeModel == null || pincodeModel.Branch == null)
                {
                    validationErrors.Add("Sorry, we dont serve in this area");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                user.BranchId = pincodeModel.BranchId;
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new CommandResult(System.Net.HttpStatusCode.OK,
                    new BranchModel { Id = (int)pincodeModel.BranchId, Name = pincodeModel.Branch.Name });
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ProfileMgr.SetBranchByPincode Exception : userId: {userId} pincode: {pincode} " + ex.Message);
                validationErrors.Add("Some internal error has occurred " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> UpdateDeviceId(UpdateDeviceIdModel model)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(model.NewDeviceId))
                {
                    validationErrors.Add("Device Id is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                model.UserId  = model.UserId > 0 ? model.UserId : null;

                if (model.UserId != null && model.UserId > 0)
                {
                    var userProfile = _db.Profiles.Where(x => x.UserId == model.UserId).FirstOrDefault();
                    if (userProfile != null)
                    {
                        userProfile.DeviceId = model.NewDeviceId;
                        _db.Entry(userProfile).State = EntityState.Modified;
                    }
                    else
                    {
                        validationErrors.Add("User is invalid");
                        return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                    }
                }

                UserDevice existingOld = null;
                if (!string.IsNullOrEmpty(model.OldDeviceId))
                {
                    existingOld = _db.UserDevices.Where(p => p.FirebaseDeviceId == model.OldDeviceId).FirstOrDefault();
                }
                if (existingOld == null)
                {
                    UserDevice existingNew = _db.UserDevices.Where(p => p.FirebaseDeviceId == model.NewDeviceId).FirstOrDefault();
                    if (existingNew == null)
                    {
                        existingNew = new UserDevice();
                        existingNew.FirebaseDeviceId = model.NewDeviceId;
                        existingNew.UserId = model.UserId;
                        existingNew.Source = model.Source;
                        _db.UserDevices.Add(existingNew);
                    }
                    else
                    {
                        existingNew.UserId = model.UserId;
                        existingNew.Source = model.Source;
                        _db.Entry(existingNew).State = EntityState.Modified;
                    }
                }
                else
                {
                    existingOld.FirebaseDeviceId = model.NewDeviceId;
                    existingOld.UserId = model.UserId;
                    existingOld.Source = model.Source;
                    _db.Entry(existingOld).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();

                return new CommandResult(System.Net.HttpStatusCode.OK, new { message = "Device Id updated successfully" });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Exception " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> UpdateCustomerProfile(int userId, UpdateCustomerProfileModel updateProfile)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var existingUser = await _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    validationErrors.Add("Customer is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                existingUser.Profile.FirstName = updateProfile.FirstName;
                existingUser.Profile.LastName = updateProfile.LastName;
                existingUser.Profile.Email = updateProfile.Email;
                existingUser.Profile.SendNotifications = updateProfile.SendNotifications;

                _db.Entry(existingUser).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                var customerProfile = await _profileQueries.GetCustomerProfileByUserId(userId);

                return new CommandResult(System.Net.HttpStatusCode.OK, customerProfile);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ProfileMgr.UpdateCustomerProfile Exception : userId: {userId} | " + ex.Message);
                validationErrors.Add("Some internal error has occurred " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> UpdateDriverProfile(int userId, UpdateDriverProfileModel updateProfile)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var existingUser = await _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    validationErrors.Add("Driver is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                existingUser.Profile.FirstName = updateProfile.FirstName;
                existingUser.Profile.LastName = updateProfile.LastName;
                existingUser.Profile.Email = updateProfile.Email;

                _db.Entry(existingUser).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                var driverProfile = await _profileQueries.GetDriverProfileByUserId(userId);

                return new CommandResult(System.Net.HttpStatusCode.OK, driverProfile);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"ProfileMgr.UpdateDriverProfile Exception : userId: {userId} | " + ex.Message);
                validationErrors.Add("Some internal error has occurred " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> UpdateDriverMobileSendOtp(int userId, UpdateMobileGetOTPRequest request)
        {
            List<string> errors = new List<string>();

            UserAndProfileModel userProfileModel = await _profileQueries.GetByUserId(userId);
            if (userProfileModel.UserId <= 0)
            {
                errors.Add("Driver is invalid");
                return CommandResult.FromValidationErrors(errors);
            }
            else
            {
                UserAndProfileModel newMobileUserProfileModel = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.DRIVER, userProfileModel.TenantId);
                if (newMobileUserProfileModel.UserId > 0)
                {
                    errors.Add("Driver with mobile " + request.Mobile + " already exists");
                    return CommandResult.FromValidationErrors(errors);
                }
                else
                {
                    userProfileModel.Mobile = request.Mobile;
                    Otp otpModel = await _otpMgr.SendChangeMobileOtp(userProfileModel);
                    if (otpModel.Id > 0)
                    {
                        UpdateMobileGetOTPResponse response = new UpdateMobileGetOTPResponse();
                        response.Mobile = request.Mobile;
                        response.OTPUniqueId = otpModel.UniqueId;
                        return new CommandResult(System.Net.HttpStatusCode.OK, response);
                    }
                }
            }
            return CommandResult.FromValidationErrors();
        }

        public async Task<CommandResult> UpdateDriverMobileReSendOtp(int userId, LoginByOTPResendRequest request)
        {
            UserAndProfileModel userProfileModel = await _profileQueries.GetByUserId(userId);
            userProfileModel.Mobile = request.Mobile;

            Otp otpModel = await _otpMgr.ReSendChangeMobileOtp(userProfileModel, request.OTPUniqueId);
            if (otpModel.Id > 0)
            {
                UpdateMobileGetOTPResponse response = new UpdateMobileGetOTPResponse();
                response.Mobile = request.Mobile;
                response.OTPUniqueId = otpModel.UniqueId;
                return new CommandResult(System.Net.HttpStatusCode.OK, response);
            }

            return CommandResult.FromValidationErrors();
        }

        public async Task<CommandResult> UpdateCustomerMobileSendOtp(int userId, UpdateMobileGetOTPRequest request)
        {
            List<string> errors = new List<string>();

            UserAndProfileModel userProfileModel = await _profileQueries.GetByUserId(userId);
            if (userProfileModel.UserId <= 0)
            {
                errors.Add("Customer is invalid");
                return CommandResult.FromValidationErrors(errors);
            }
            else
            {
                UserAndProfileModel newMobileUserProfileModel = await _profileQueries.GetUserAndProfileByMobile(request.Mobile, UserType.CUSTOMER, userProfileModel.TenantId);
                if (newMobileUserProfileModel.UserId > 0)
                {
                    errors.Add("Customer with mobile " + request.Mobile + " already exists");
                    return CommandResult.FromValidationErrors(errors);
                }
                else
                {
                    userProfileModel.Mobile = request.Mobile;
                    Otp otpModel = await _otpMgr.SendChangeMobileOtp(userProfileModel);
                    if (otpModel.Id > 0)
                    {
                        UpdateMobileGetOTPResponse response = new UpdateMobileGetOTPResponse();
                        response.Mobile = request.Mobile;
                        response.OTPUniqueId = otpModel.UniqueId;
                        return new CommandResult(System.Net.HttpStatusCode.OK, response);
                    }
                }
            }
            return CommandResult.FromValidationErrors();
        }

        public async Task<CommandResult> UpdateCustomerMobileReSendOtp(int userId, LoginByOTPResendRequest request)
        {
            UserAndProfileModel userProfileModel = await _profileQueries.GetByUserId(userId);
            userProfileModel.Mobile = request.Mobile;

            Otp otpModel = await _otpMgr.ReSendChangeMobileOtp(userProfileModel, request.OTPUniqueId);
            if (otpModel.Id > 0)
            {
                UpdateMobileGetOTPResponse response = new UpdateMobileGetOTPResponse();
                response.Mobile = request.Mobile;
                response.OTPUniqueId = otpModel.UniqueId;
                return new CommandResult(System.Net.HttpStatusCode.OK, response);
            }

            return CommandResult.FromValidationErrors();
        }

        public async Task<CommandResult> MaskAsAmbassador(MarkAmbassadorModel profile)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var user = _db.Users.Include(p => p.Profile).Where(p => p.Id == profile.UserId).FirstOrDefault();
                if (user == null)
                {
                    validationErrors.Add("User is invalid ");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (user.Profile.MyReferralCode.ToLower().StartsWith(_reservedAmbReferralCodeStarting))
                {
                    validationErrors.Add("User is already a Ambassador ");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                user.Profile.MyReferralCode = GenerateAmbassadorReferralCode();
                _db.Entry(user.Profile).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, "Customer marked as Ambassador");
            }
            catch (Exception ex)
            {
                validationErrors.Add("Sorry, some internal error has occured. ");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

        }

        public async Task<CommandResult> MarkProfileValidated(ValidateProfileModel request)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var user = _db.Users.Where(p => p.Id == request.UserId && p.Type == UserType.CUSTOMER).FirstOrDefault();
                if (user == null)
                {
                    validationErrors.Add("User is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (user.OtpValidated)
                {
                    validationErrors.Add("User is already validated");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                user.OtpValidated = true;
                user.OtpValidatedAt = DateMgr.GetCurrentIndiaTime();
                user.OtpValidatedBy = request.OtpValidatedBy;
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, "Profile validated successfully");
            }
            catch (Exception ex)
            {
                validationErrors.Add("Sorry, some internal error has occured.");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<CommandResult> AttachDistributorToCustomer(AttachDistributorModel request, int userId)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var user = _db.Users.Where(x => x.Id == request.UserId && x.Type == UserType.CUSTOMER).FirstOrDefault();
                var distributor = _db.BusinessEntities.Where(x => x.Id == request.DistributorId && x.Type == BusinessEntityType.Distributor).FirstOrDefault();

                if (user == null)
                {
                    validationErrors.Add("User is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                if (distributor == null)
                {
                    validationErrors.Add("Distributor is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                user.BusinessEntityId = distributor.Id;
                user.BusinessEntityAttachedAt = DateMgr.GetCurrentIndiaTime();
                user.BusinessEntityAttachedByUserId = userId;
                user.UpdatedBy = userId.ToString();
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation("AttachDistributorToCustomer {@businessEntity} successfull by backend user {updatedBy}", request, userId);

                var orderUpdateResponse = await _orderService.UpdateAttachedDistributorForActiveOrders(user.Id,
                    new AttachDistributorToOrderRequest
                    {
                        AttachedBusinessEntityId = distributor.Id,
                        AttachedBusinessEntityName = distributor.Name
                    });

                return new CommandResult(System.Net.HttpStatusCode.OK, new { message = "Distributor attached successfully" });
            }
            catch (Exception ex)
            {
                validationErrors.Add("Exception " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<bool> UpdateAverageRating(int id, float rating, UserType userType)
        {
            if (userType == UserType.RELAY_POINT)
            {
                var relaypoint = await _db.BusinessEntities
                    .Where(p => p.Type == BusinessEntityType.Relaypoint && p.Id == id)
                    .FirstOrDefaultAsync();

                if (relaypoint != null)
                {
                    relaypoint.Rating = (float)Math.Round(rating, 2);
                    _db.Entry(relaypoint).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _logger.LogCritical("Profile.API UpdateAverageRating invalid relaypoint for {relaypointId}", id);
                }
            }
            else
            {
                var userProfile = await _db.Profiles
                    .Where(p => p.UserId == id)
                    .FirstOrDefaultAsync();

                if (userProfile != null)
                {
                    userProfile.Rating = (float)Math.Round(rating, 2);
                    _db.Entry(userProfile).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _logger.LogCritical("Profile.API UpdateAverageRating invalid userProfile for {userId}", id);
                }
            }

            return true;
        }

        private string GenerateAmbassadorReferralCode()
        {
            string myReferralCode = "";
            try
            {
                var ambassdorList = _db.Profiles.Where(p => p.MyReferralCode.StartsWith(_reservedAmbReferralCodeStarting)).ToList();
                int ambCount = ambassdorList.Count;
                ambCount += 1;
                myReferralCode = _reservedAmbReferralCodeStarting + ambCount.ToString().PadLeft(3, '0');

                while (_db.Profiles.Any(p => p.MyReferralCode == myReferralCode))
                {
                    ambCount += 1;
                    myReferralCode = _reservedAmbReferralCodeStarting + ambCount.ToString().PadLeft(3, '0');
                }
            }
            catch (Exception ex)
            {

            }
            return myReferralCode;
        }

        #region Relaypoint

        public async Task<CommandResult> UpdateRelaypointWorkingDays(int relaypointId, UpdateWorkingDaysRequest request)
        {
            List<string> validationErrors = new List<string>();

                var businessEntity = await _db.BusinessEntities
                    .Include(p => p.Timings)
                    .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                    .FirstOrDefaultAsync();

                if (businessEntity == null)
                {
                    validationErrors.Add("Relaypoint is invalid");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

            if (businessEntity.Timings != null)
            {
                foreach (var workingDay in businessEntity.Timings)
                {
                    _db.BusinessEntityTimings.Remove(workingDay);
                }
            }
               
                foreach (var workingDayReq in request.WorkingDaysList)
                {
                    var workingDay = new BusinessEntityTiming()
                    {
                        BusinessEntityId = relaypointId,
                        Day = workingDayReq.Day,
                        IsActive = workingDayReq.IsActive,
                    };
                    _db.BusinessEntityTimings.Add(workingDay);
                }

                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Working Days updated successfully"));
        }

        public async Task<CommandResult> UpdateRelaypointWorkingTime(int relaypointId, UpdateWorkingTimeRequest request)
        {
            List<string> validationErrors = new List<string>();

            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                validationErrors.Add("Relaypoint is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            businessEntity.WorkingStartTime = request.StartTime;
            businessEntity.WorkingEndTime = request.EndTime;
            _db.Entry(businessEntity).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Working Time updated successfully"));
        }

        public async Task<CommandResult> UpdateRelaypointProfile(int relaypointId, UpdateRelaypointProfile request)
        {
            List<string> validationErrors = new List<string>();

            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                validationErrors.Add("Relaypoint is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            businessEntity.Name = request.Name;
            businessEntity.MobileNumber = request.MobileNumber;
            businessEntity.Email = request.Email;

            _db.Entry(businessEntity).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var profile = new RelaypointProfile()
            {
                Name = businessEntity.Name,
                MobileNumber = businessEntity.MobileNumber,
                Email = businessEntity.Email,
                Rating = 5 //TODO
            };

            return new CommandResult(System.Net.HttpStatusCode.OK, profile);
        }

        public async Task<CommandResult> UpdateRelaypointAddress(int relaypointId, UpdateRelaypointAddress request)
        {
            List<string> validationErrors = new List<string>();

            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                validationErrors.Add("Relaypoint is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            businessEntity.Location = request.Location;
            businessEntity.Lat = request.Lat;
            businessEntity.Lng = request.Lng;
            businessEntity.Details = request.Details;
            businessEntity.PinCode = request.PinCode;
            businessEntity.GeoLocation = geometryFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));

            _db.Entry(businessEntity).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Address updated successfully"));
        }

        public async Task<CommandResult> SendOrdersAttachmentToRelaypointEmail(int relaypointId, SendRelaypointEmailRequest request)
        {
            var relaypoint = await _db.BusinessEntities
                .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                .FirstOrDefaultAsync();

            if (relaypoint == null)
            {
                return CommandResult.FromValidationErrors("Relaypoint is invalid");
            }
            if (string.IsNullOrEmpty(relaypoint.Email))
            {
                return CommandResult.FromValidationErrors("Please update your email in your profile and try again.");
            }

            string subject = "EasyGas Booking Report " + request.FromDate.ToString("dd/MM/yyy") + " - " + request.ToDate.ToString("dd/MM/yyy");
            string body = "EasyGas Booking report for relaypoint " + relaypoint.Name + " during " + request.FromDate.ToString("dd/MM/yyy") + " - " + request.ToDate.ToString("dd/MM/yyy") + " is attached as requested in EasyGas Kenya Relaypoint app.<br/><br/><br/> Thank you,<br/>Admin<br/>";
            bool isEmailSend = await _emailSender.SendEmailAsync(relaypoint.Name, relaypoint.Email, subject, body, new List<string>() { request.AttachmentUrl });
            if (isEmailSend)
            {
                return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Email successfully sent to " + relaypoint.Email));
            }
            else
            {
                return CommandResult.FromValidationErrors("Could not send the email. Please try again later.");
            }
        }

        public async Task<CommandResult> SendStockRequestEmailToAdmin(int relaypointId, SendStockRequestEmailToAdmin request)
        {
            var relaypoint = await _db.BusinessEntities
                .Include(p => p.Branch)
                .Where(p => p.Id == relaypointId && p.Type == BusinessEntityType.Relaypoint)
                .FirstOrDefaultAsync();

            if (relaypoint == null)
            {
                return CommandResult.FromValidationErrors("Relaypoint is invalid");
            }
            if (string.IsNullOrEmpty(relaypoint.Branch.Email))
            {
                return CommandResult.FromValidationErrors("Admin email address not found.");
            }

            string subject = "EasyGas Stock Request by relaypoint " + relaypoint.Name;
            string body = "<style>table td, table th { padding: 10px} </style>";
            body += "Hi, <br/>";
            body += relaypoint.Name + " has requested stock for the following items-<br/><br/>";
            body += "<table border='1' ><thead><tr><th>Item</th><th>Requested Qty</th></tr></thead><tbody>";
            foreach(var item in request.OrderRequests)
            {
                body += "<tr><td>" + item.Item + "</td><td>" + item.Quantity + "</td></tr>";
            }
            body += "</tbody></table>";
            body += "<br/><br/><br/>Thank you,<br/>Admin";

            bool isEmailSend = await _emailSender.SendEmailAsync(relaypoint.Branch.Email, "Admin", subject, body, new List<string>());
            if (isEmailSend)
            {
                return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Email successfully sent to " + relaypoint.Branch.Email));
            }
            else
            {
                return CommandResult.FromValidationErrors("Could not send order request email to admin. Please try again later.");
            }
        }

        #endregion

        #region BusinessEntity General

        public async Task<CommandResult> UpdateBusinessEntityProfile(int id, UpdateDistributorProfile request)
        {
            List<string> validationErrors = new List<string>();

            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                validationErrors.Add("Entity is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            businessEntity.Name = request.Name;
            businessEntity.MobileNumber = request.MobileNumber;
            businessEntity.Email = request.Email;
            businessEntity.PaymentNumber = request.UpiPaymentNumber;
            businessEntity.GSTN = request.GSTN;
            businessEntity.PAN = request.PAN;
            _db.Entry(businessEntity).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var profile = new DistributorProfile()
            {
                Name = businessEntity.Name,
                Code = businessEntity.Code,
                GSTN = businessEntity.GSTN,
                PAN = businessEntity.PAN,
                UpiPaymentNumber = businessEntity.PaymentNumber,
                MobileNumber = businessEntity.MobileNumber,
                Email = businessEntity.Email,
                Rating = 5 //TODO
            };

            return new CommandResult(System.Net.HttpStatusCode.OK, profile);
        }

        public async Task<CommandResult> UpdateBusinessEntityAddress(int id, UpdateDistributorAddress request)
        {
            List<string> validationErrors = new List<string>();

            var businessEntity = await _db.BusinessEntities
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (businessEntity == null)
            {
                validationErrors.Add("Entity is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            businessEntity.Location = request.Location;
            businessEntity.Lat = request.Lat;
            businessEntity.Lng = request.Lng;
            businessEntity.Details = request.Details;
            businessEntity.PinCode = request.PinCode;
            businessEntity.GeoLocation = geometryFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));

            _db.Entry(businessEntity).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Address updated successfully"));
        }

        #endregion

        public async Task<CommandResult> CreateCustomerAddress(int userId, int customerId, AddressModel request)
        {
            List<string> validationErrors = new List<string>();

            var user = _db.Users.Include(p => p.Profile).Where(x => x.Id == customerId).FirstOrDefault();
            if (user == null)
            {
                return CommandResult.FromValidationErrors($"User not found");
            }

            UserAddress address = new UserAddress()
            {
                UserId = user.Id,
                BranchId = request.BranchId,
                TenantId = (int)request.TenantId,
                City = request.City,
                Details = request.Details,
                Landmark = request.Landmark,
                Lat = request.Lat,
                Lng = request.Lng,
                Location = request.Location,
                Name = request.Name,
                PhoneAlternate = request.PhoneAlternate,
                PinCode = request.PinCode,
                State = request.State,
                CreatedAt = DateMgr.GetCurrentIndiaTime(),
                CreatedBy = userId.ToString()
            };

            _db.UserId = userId.ToString();
            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            return new CommandResult(System.Net.HttpStatusCode.OK, new CreateUserAddressResponse { UserAddressId = address.Id });
        }
    }
}

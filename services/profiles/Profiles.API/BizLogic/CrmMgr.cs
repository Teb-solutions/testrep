using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;
using EasyGas.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Profiles.API.Infrastructure.Services;
using Profiles.API.Models;
using Profiles.API.Services;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Account;
using Profiles.API.ViewModels.Crm;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.ViewModels.PulzConnect;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class CrmMgr
    {
        private ProfilesDbContext _db;
        private ISmsSender _smsSender;
        private IEmailSender _emailSender;
        private IProfileQueries _profileQueries;
        private ICrmApiService _crmApiService;
        private IIdentityService _identityService;
        private OtpMgr _otpMgr;
        private string _reservedAmbReferralCodeStarting;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ILogger<CrmMgr> _logger;

        public CrmMgr(ProfilesDbContext db, IOptions<ApiSettings> apiSettings, ISmsSender smsSender,
            IProfileQueries profileQueries, IEmailSender emailSender,
            ICrmApiService crmApiService, IIdentityService identityService,
            OtpMgr otpMgr, ILogger<CrmMgr> logger)
        {
            _db = db;
            _smsSender = smsSender;
            _profileQueries = profileQueries;
            _otpMgr = otpMgr;
            _crmApiService = crmApiService;
            _identityService = identityService;
            _apiSettings = apiSettings;
            _emailSender = emailSender;
            _reservedAmbReferralCodeStarting = _apiSettings.Value.AmbassadorReferralCodeStartsWith.ToLower();
            _logger = logger;
        }

        public async Task<CommandResult> Register(CreateCrmStaffRequest request)
        {
            if (request == null)
            {
                return CommandResult.FromValidationErrors("Invalid data ");
            }

            var existing = _db.Users.Any(x => x.UserName == request.UserName && x.Type == UserType.CUSTOMER_CARE);
            if (existing)
            {
                return CommandResult.FromValidationErrors($"Username ({request.UserName}) already exists.");
            }

            var tenant = _db.Tenants.FirstOrDefault();
            if (tenant == null)
            {
                return CommandResult.FromValidationErrors($"Tenant is invalid.");
            }

            var role = _db.Roles.Where(p => p.Name == request.Role).FirstOrDefault();
            if (role == null)
            {
                return CommandResult.FromValidationErrors($"Role is not added.");
            }

            User user = new User()
            {
                TenantId = tenant.Id,
                CreationType = CreationType.USER,
                OtpValidated = true,
                UserName = request.UserName,
                Type = UserType.CUSTOMER_CARE,
                Profile = new UserProfile()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    AgreedTerms = true,
                    Code = request.Mobile,
                    Email = request.Email,
                    Mobile = request.Mobile,
                    Source = Shared.Source.CRM
                }
            };
            user.Roles.Add(new UserRole(role.Id));

            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordPolicyErrors = _identityService.ValidatePasswordPolicy(user.Type, request.Password);
                if (passwordPolicyErrors.Count() > 0)
                {
                    return CommandResult.FromValidationErrors(passwordPolicyErrors);
                }

                user.Password = Helper.HashPassword(request.Password);
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var crmApiResponse = await _crmApiService.CreateStaff(request, user.Id);
            _logger.LogInformation("Crm New User Created | username: " + request.UserName);

            return new CommandResult(HttpStatusCode.OK, new CreateProfileResponse { UserId = user.Id });
        }


    }
}

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
using Profiles.API.Models;
using Profiles.API.Services;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.ViewModels.PulzConnect;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class PulzConnectMgr
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

        public PulzConnectMgr(ProfilesDbContext db, IOptions<ApiSettings> apiSettings, ISmsSender smsSender,
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

        public async Task<ProfileModel> GetCustomerProfileByMobile(string mobile)
        {
            var profile = await _db.Profiles
                .Include(p => p.User)
                .Where(p => p.User.Type == UserType.CUSTOMER && p.Mobile == mobile)
                .FirstOrDefaultAsync();

            if (profile != null)
            {
                return ProfileModel.FromProfile(profile);
            }
            else return null;
        }

       
    }
}

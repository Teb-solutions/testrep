using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class VoucherMgr
    {
        private HttpClient _apiClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private ProfilesDbContext _db;
        private NotificationMgr _notiMgr;
        private OtpMgr _otpMgr;

        private readonly ILogger _logger;

        private string[] _alpaNumericCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private string _reservedAmbReferralCodeStarting;

        public VoucherMgr(IOptions<ApiSettings> apiSettings, ProfilesDbContext db, NotificationMgr notiMgr, ILoggerFactory loggerFactory, OtpMgr otpMgr)
        {
            _apiSettings = apiSettings;
            _db = db;
            _notiMgr = notiMgr;
            _otpMgr = otpMgr;
            _logger = loggerFactory.CreateLogger<VoucherMgr>();
            _reservedAmbReferralCodeStarting = _apiSettings.Value.AmbassadorReferralCodeStartsWith.ToLower();
        }

        public async Task<CommandResult> CreateCustomerRefferalCode(int userId)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var userProfile = _db.Profiles.Where(p => p.UserId == userId).FirstOrDefault();
                string referralCode = GenerateRandomAlphaNumericString(5);

                while(_db.Profiles.Any(p => p.MyReferralCode == referralCode))
                {
                    referralCode = GenerateRandomAlphaNumericString(5);
                }

                userProfile.MyReferralCode = referralCode;
                _db.Entry(userProfile).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("CreateCustomerRefferalCode " + userId + "/" + ex.ToString());

            }

            validationErrors.Add("Sorry, some error has occured. Please try after some time.");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        public string GenerateMyReferralCode()
        {
            string myReferralCode = "";
            try
            {
                myReferralCode = GenerateRandomAlphaNumericString(5);

                while (_db.Profiles.Any(p => p.MyReferralCode == myReferralCode) || myReferralCode.StartsWith(_reservedAmbReferralCodeStarting))
                {
                    myReferralCode = GenerateRandomAlphaNumericString(5);
                }
            }
            catch(Exception ex)
            {
                _logger.LogCritical("VoucherMgr.GenerateMyReferralCode {referralCode} {@exeption}", myReferralCode, ex);
            }
            return myReferralCode;
        }



        public string GenerateRandomAlphaNumericString(int length)
        {
            string randomString = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                sTempChars = _alpaNumericCharacters[rand.Next(0, _alpaNumericCharacters.Length)];
                randomString += sTempChars.ToLower();
            }
            return randomString;
        }

        public async Task<CommandResult> UpdateMissingReferralCodes()
        {
            List<string> validationErrors = new List<string>();
            int count = 0;
            try
            {
                var profiles = _db.Profiles.Where(p => p.MyReferralCode == null).ToList();
                foreach (var profile in profiles)
                {
                    if (string.IsNullOrEmpty(profile.MyReferralCode))
                    {
                        profile.MyReferralCode = GenerateMyReferralCode();
                        _db.Entry(profile).State = EntityState.Modified;
                        count += 1;
                    }
                }
                await _db.SaveChangesAsync();
                return new CommandResult(System.Net.HttpStatusCode.OK, "added " + count + " referral codes");
            }
            catch (Exception ex)
            {
                validationErrors.Add("Sorry, some error has occured. " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

        }

    }
}

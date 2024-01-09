using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Formatters;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class OtpMgr
    {
        private ProfilesDbContext _db;
        private ISmsSender _smsSender;
        private string[] digitCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        private string[] _alpaNumericCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public OtpMgr(ProfilesDbContext db, ISmsSender smsSender)
        {
            _db = db;
            _smsSender = smsSender;
        }

        public async Task<Otp> SendCustomerRegistrationOtp(UserAndProfileModel userProfile)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = GenerateRandomOTP(4, digitCharacters);
                var smsSend = true;

                // for tsting
                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForRegisterOtp(userProfile.Mobile, userProfile.FirstName, otp, userProfile.TenantId);
                }
                if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        UniqueId = Guid.NewGuid().ToString(),
                        TenantId = userProfile.TenantId,
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_REGISTER,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                }
            }
            catch(Exception ex)
            {

            }
            
            return responseOtpModel;
        }

        public async Task<Otp> SendResetPasswordOtp(UserAndProfileModel userProfile)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = GenerateRandomOTP(4, digitCharacters);
                var smsSend = true;

                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForResetPasswordOtp(userProfile, otp);
                }

                if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        UniqueId = Guid.NewGuid().ToString(),
                        TenantId = userProfile.TenantId,
                        UserId = userProfile.UserId,
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_RESET_PASSWORD,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                    responseOtpModel.Value = "";
                }
            }
            catch (Exception ex)
            {

            }

            return responseOtpModel;
        }

        public async Task<Otp> SendLoginOtp(UserAndProfileModel userProfile)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = "";
                DateTime lastOtpMinTime = DateMgr.GetCurrentIndiaTime().AddMinutes(-180);
                var lastSendOtp = _db.Otps.Where(p => p.UserId == userProfile.UserId && p.Type == OtpType.CUSTOMER_LOGIN && p.CreatedAt >= lastOtpMinTime).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (lastSendOtp != null)
                {
                    otp = lastSendOtp.Value;
                }
                if (string.IsNullOrEmpty(otp))
                {
                    otp = GenerateRandomOTP(4, digitCharacters);
                }

                var smsSend = true;
                // TODO remove after tsting
                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForLoginOtp(userProfile, otp);
                }
                //if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        UniqueId = Guid.NewGuid().ToString(),
                        TenantId = userProfile.TenantId,
                        UserId = userProfile.UserId,
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_LOGIN,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                    //responseOtpModel.Value = "";
                }
            }
            catch (Exception ex)
            {

            }

            return responseOtpModel;
        }

        public async Task<Otp> ReSendLoginOtp(UserAndProfileModel userProfile, string otpUniqueId)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = "";
                //DateTime lastOtpMinTime = DateMgr.GetCurrentIndiaTime().AddMinutes(-180);
                var lastSendOtp = _db.Otps.Where(p => p.UniqueId == otpUniqueId).FirstOrDefault();
                if (lastSendOtp != null)
                {
                    otp = lastSendOtp.Value;
                }
                if (string.IsNullOrEmpty(otp))
                {
                    otp = GenerateRandomOTP(4, digitCharacters);
                }

                var smsSend = true;
                // TODO remove after tsting
                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForLoginOtp(userProfile, otp);
                }

                //if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        UniqueId = Guid.NewGuid().ToString(),
                        TenantId = userProfile.TenantId,
                        UserId = userProfile.UserId,
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_LOGIN,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                    //responseOtpModel.Value = "";
                }
            }
            catch (Exception ex)
            {

            }

            return responseOtpModel;
        }

        public async Task<Otp> SendChangeMobileOtp(UserAndProfileModel userProfile)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = GenerateRandomOTP(4, digitCharacters);

                var smsSend = true;
                // TODO remove after tsting
                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForChangeMobileOtp(userProfile, otp);
                }

                if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        TenantId = userProfile.TenantId,
                        UserId = userProfile.UserId,
                        UniqueId = Guid.NewGuid().ToString(),
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_CHANGE_MOBILE,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                }
            }
            catch (Exception ex)
            {

            }

            return responseOtpModel;
        }

        public async Task<Otp> ReSendChangeMobileOtp(UserAndProfileModel userProfile, string otpUniqueId)
        {
            Otp responseOtpModel = new Otp();
            try
            {
                string otp = "";
                //DateTime lastOtpMinTime = DateMgr.GetCurrentIndiaTime().AddMinutes(-180);
                var lastSendOtp = _db.Otps.Where(p => p.UniqueId == otpUniqueId).FirstOrDefault();
                if (lastSendOtp != null)
                {
                    otp = lastSendOtp.Value;
                }
                if (string.IsNullOrEmpty(otp))
                {
                    otp = GenerateRandomOTP(4, digitCharacters);
                }

                var smsSend = true;
                // TODO remove after tsting
                if (userProfile.Mobile == "1122334455")
                {
                    otp = "0000";
                }
                else
                {
                    smsSend = await _smsSender.SendSmsToCustomerForChangeMobileOtp(userProfile, otp);
                }
                
                if (smsSend)
                {
                    Otp otpModel = new Otp
                    {
                        TenantId = userProfile.TenantId,
                        UserId = userProfile.UserId,
                        UniqueId = Guid.NewGuid().ToString(),
                        Receiver = OtpReceiver.CUSTOMER,
                        Type = OtpType.CUSTOMER_CHANGE_MOBILE,
                        Value = otp,
                        HasExpiry = false,
                        IsValidated = false,
                        SendTo = userProfile.Mobile
                    };
                    _db.Add(otpModel);
                    await _db.SaveChangesAsync();
                    responseOtpModel = otpModel;
                }
            }
            catch (Exception ex)
            {

            }

            return responseOtpModel;
        }

        public bool ValidateOTP(string otpUniqueId, string otpValue, string mobile)
        {
            bool isValid = false;
            try
            {
                Otp otpModel = _db.Otps.Where(p => p.UniqueId == otpUniqueId && p.SendTo == mobile).FirstOrDefault();
                if (otpModel != null)
                {
                    isValid = otpModel.Value == otpValue;
                    if (isValid)
                    {
                        otpModel.IsValidated = true;
                        _db.Entry(otpModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _db.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return isValid;
        }

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        public string GenerateRandomNumbersOTP(int iOTPLength)
        {
            return GenerateRandomOTP(iOTPLength, digitCharacters);
        }

        public string GenerateRandomAlphaNumericString(int length)
        {
            string randomString = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                sTempChars = _alpaNumericCharacters[rand.Next(0, _alpaNumericCharacters.Length)];
                randomString += sTempChars;
            }
            return randomString;
        }
    }
}

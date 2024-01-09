using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Profiles.API.ViewModels.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public class WalletApiService : IWalletService
    {
        private HttpClient _apiClient;
        private readonly ProfilesDbContext _db;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ILogger _logger;

        public WalletApiService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ProfilesDbContext ctx, ILogger<WalletApiService> logger )
        {
            _apiSettings = apiSettings;
            _db = ctx;
            _logger = logger;
            _apiClient = httpClient;
        }

        public async Task<bool> CreateWallet(WalletVM wallet)
        {
            ApiValidationErrors errors = new ApiValidationErrors() {  };

            try
            {
                    var uri = string.Format("{0}{1}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.CreateWallet);

                    var response = await _apiClient.PostAsJsonAsync(uri, wallet);
                    if (response.IsSuccessStatusCode)
                    {
                    _logger.LogInformation("WalletService.CreateCustomerWallet API Response for {url} Success for {@wallet} ", uri, wallet);
                        return true;
                    }
                    else
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                        _logger.LogCritical("WalletService.CreateCustomerWallet service error for wallet {@wallet} with {@errors}",  wallet, errors);
                    }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("WalletService.CreateCustomerWallet exception {@exception} for wallet {@wallet}", ex, wallet);
            }
            return false;
        }

        public async Task<UserCouponSummary> GetCouponSummary(int userId)
        {
            try
            {
                string tenantUID = _apiSettings.Value.TenantUidDefault;
                var uri = string.Format("{0}{1}?userUniqueId={2}&walletType={3}&tenantUID={4}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.GetCouponSummary,
                    userId, WalletType.ENDUSER, tenantUID);

                var response = await _apiClient.GetAsync(uri);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var couponSummary = JsonConvert.DeserializeObject<UserCouponSummary>(responseJson);
                    _logger.LogInformation("WalletService.GetCouponSummary API Response for {url} Success {@summary} ", uri, couponSummary);
                    return couponSummary;
                }
                else
                {
                    ApiValidationErrors errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                    _logger.LogCritical("WalletService.GetCouponSummary service error | userId " + userId + " | " + errors.Err.ToString());
                }

            }
            catch (Exception)
            {

            }

            return null;
        }

        public async Task<CouponModel> GetActiveReferralCoupon()
        {
            try
            {
                string tenantUID = _apiSettings.Value.TenantUidDefault;
                var uri = string.Format("{0}{1}?tenantUID={2}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.GetActiveReferralCoupon, tenantUID);

                var response = await _apiClient.GetAsync(uri);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var couponSummary = JsonConvert.DeserializeObject<CouponModel>(responseJson);
                    return couponSummary;
                }
                else
                {
                    //ApiValidationErrors errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                }

            }
            catch (Exception)
            {

            }

            return null;
        }

        public async Task<bool> OrderCreationTransaction(WalletTransactionCouponVM walletTrans)
        {
            ApiValidationErrors errors = new ApiValidationErrors() { };
            try
            {
                    var uri = string.Format("{0}{1}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.OrderCreationTransaction);

                    var response = await _apiClient.PostAsJsonAsync(uri, walletTrans);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                    _logger.LogInformation("WalletService.OrderCreationTransaction API Response for {url} Success for {@wallet} ", uri, walletTrans);
                        return true;
                    }
                    else
                    {
                        errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                    _logger.LogCritical("WalletService.OrderCreationTransaction API Response {url} {@Error} for {@wallet}", uri, errors, walletTrans);
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> OrderCompletionTransaction(WalletOrderCouponCompletionVM walletTrans)
        {
            ApiValidationErrors errors = new ApiValidationErrors() { };
            try
            {
                    var uri = string.Format("{0}{1}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.OrderCompletionTransaction);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, walletTrans);
                    if (response.IsSuccessStatusCode)
                    {
                    _logger.LogInformation("WalletService.OrderCompletionTransaction API Response for {url} Success for {@wallet} ", uri, walletTrans);
                        return true;
                    }
                    else
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                    _logger.LogCritical("WalletService.OrderCompletionTransaction API Response {url} {@Error} for {@wallet}", uri, errors, walletTrans);
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> OrderCancelTransaction(WalletTransactionCouponVM walletTrans)
        {
            ApiValidationErrors errors = new ApiValidationErrors() { };
            try
            {
                    var uri = string.Format("{0}{1}", _apiSettings.Value.WalletServiceUrl, _apiSettings.Value.OrderCancelTransaction);

                    _apiClient.DefaultRequestHeaders.Accept.Clear();
                    var response = await _apiClient.PostAsJsonAsync(uri, walletTrans);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("WalletService.OrderCancelTransaction API Response for {url} Success for {@wallet} ", uri, walletTrans);
                        return true;
                    }
                    else
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        errors = JsonConvert.DeserializeObject<ApiValidationErrors>(responseJson);
                        _logger.LogCritical("WalletService.OrderCancelTransaction API Response {url} {@Error} for {@wallet}", uri, errors, walletTrans);
                    }
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}

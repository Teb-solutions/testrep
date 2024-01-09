using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Broadcast;
using Newtonsoft.Json;

namespace Profiles.API.Services
{
    
    public class CartApiService : ICartService
    {
        private readonly HttpClient _apiClient;
        private readonly ILogger<CartApiService> _logger;
        private readonly IOptions<ApiSettings> _settings;

        public CartApiService(HttpClient httpClient, ILogger<CartApiService> logger, IOptions<ApiSettings> settings)
        {
            _apiClient = httpClient;
            _logger = logger;
            _settings = settings;
        }

        public async Task<bool> ConvertTempCart(string tempUserId, string userId)
        {
            var url = _settings.Value.CartApiUrl + _settings.Value.ConvertTempCart + tempUserId + "/" + userId;
            var response = await _apiClient.PostAsJsonAsync(url, new { });

            if (!response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                _logger.LogCritical("CartApiService ConvertTempCart Service failure " + response.StatusCode + " " + serverResponse);
            }

            return true;
        }

        public async Task<List<int>> GetCustomerIdsOrderedInBranch(int branchId)
        {
            var url = _settings.Value.OrderingApiUrl + _settings.Value.GetCustomerIdsOrderedInBranch + branchId;
            var response = await _apiClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<int>>(serverResponse);
            }

            return null;
        }

        public async Task<bool> UpdateDeliverySlots(int branchId, List<DeliverySlotModel> slots)
        {
            var url = _settings.Value.CartApiUrl + _settings.Value.UpdateDeliverySlots + branchId;
            var response = await _apiClient.PostAsJsonAsync(url, slots);

            if (!response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                _logger.LogCritical("CartApiService UpdateDeliverySlots Service failure " + response.StatusCode + " " + serverResponse);
            }

            return true;
        }
    }
}

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
using Profiles.API.ViewModels;

namespace Profiles.API.Services
{
    
    public class OrderApiService : IOrderService
    {
        private readonly HttpClient _apiClient;
        private readonly ILogger<OrderApiService> _logger;
        private readonly IOptions<ApiSettings> _settings;

        public OrderApiService(HttpClient httpClient, ILogger<OrderApiService> logger, IOptions<ApiSettings> settings)
        {
            _apiClient = httpClient;
            _logger = logger;
            _settings = settings;
        }

        public async Task<UpdateExpressBroadcastResponse> UpdateExpressOrderBroadcastDrivers(UpdateExpressBroadcastRequest req)
        {
            var url = _settings.Value.OrderingApiUrl + _settings.Value.UpdateExpressOrderBroadcastDrivers + req.OrderId;
            var response = await _apiClient.PostAsJsonAsync(url, req);
            var serverResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UpdateExpressBroadcastResponse>(serverResponse);
            }

            return null;
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

        public async Task<List<RecentCustomerOrder>> GetCustomerRecentOrders()
        {
            var url = _settings.Value.OrderingApiUrl + _settings.Value.GetCustomerRecentOrders;
            var response = await _apiClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<RecentCustomerOrder>>(serverResponse);
            }

            return null;
        }

        public async Task<bool> UpdateAttachedDistributorForActiveOrders(int customerId, AttachDistributorToOrderRequest req)
        {
            var url = _settings.Value.OrderingApiUrl + _settings.Value.AttachDistributorToActiveOrders + customerId;
            var response = await _apiClient.PutAsJsonAsync(url, req);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError("UpdateAttachedDistributorForActiveOrders OrderAPI {response} is not success for {@request} for {customerId}", serverResponse, req, customerId);
            }
            return false;
        }
    }
}

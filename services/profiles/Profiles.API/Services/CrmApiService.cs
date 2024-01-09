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
using Profiles.API.ViewModels.Crm;
using System.Net.Http.Headers;
using Org.BouncyCastle.Ocsp;

namespace Profiles.API.Services
{
    
    public class CrmApiService : ICrmApiService
    {
        private readonly HttpClient _apiClient;
        private readonly ILogger<OrderApiService> _logger;
        private readonly IOptions<ApiSettings> _settings;

        public CrmApiService(HttpClient httpClient, ILogger<OrderApiService> logger, IOptions<ApiSettings> settings)
        {
            _apiClient = httpClient;
            _logger = logger;
            _settings = settings;
        }

        public async Task<bool> CreateStaff(CreateCrmStaffRequest req, int userId)
        {
            var url = _settings.Value.CrmApiUrl + _settings.Value.CreateCrmStaff;

            object createReq = new
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                PhoneNumber = req.Mobile,
                Email = req.Email,
                userId = userId.ToString()
            };

            _apiClient.DefaultRequestHeaders.Add("tenant", "root");

            var response = await _apiClient.PostAsJsonAsync(url, createReq);
            var serverResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogCritical("CrmApiService.CreateStaff {error} creating {@staff} for {userId}", serverResponse, req, userId);
            return false;
        }

        public async Task<bool> UpdateStaffDeviceId(int userId, string deviceId, string accessToken)
        {
            var url = _settings.Value.CrmApiUrl + _settings.Value.UpdateCrmStaffDeviceId;

            object createReq = new
            {
                UserId = userId.ToString(),
                DeviceId = deviceId
            };

            HttpClient apiClient =  new HttpClient();
            apiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            apiClient.DefaultRequestHeaders.Add("tenant", "root");

            var response = await apiClient.PostAsJsonAsync(url, createReq);
            var serverResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogCritical("CrmApiService.UpdateStaffDeviceId {error} for {userId}", serverResponse, userId);
            return false;
        }
    }
}

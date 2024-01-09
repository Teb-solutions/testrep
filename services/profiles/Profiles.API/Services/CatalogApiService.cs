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
using Profiles.API.ViewModels.Distributor;
using Newtonsoft.Json;

namespace Profiles.API.Services
{
    
    public class CatalogApiService : ICatalogService
    {
        private readonly HttpClient _apiClient;
        private readonly ILogger<CatalogApiService> _logger;
        private readonly IOptions<ApiSettings> _settings;

        public CatalogApiService(HttpClient httpClient, ILogger<CatalogApiService> logger, IOptions<ApiSettings> settings)
        {
            _apiClient = httpClient;
            _logger = logger;
            _settings = settings;
        }

        public async Task<bool> AddRelaypointItemMasters(int relaypointId, int branchId)
        {
            var url = _settings.Value.CatalogApiUrl + _settings.Value.AddRelaypointItemMasters;
            url = url.Replace("{branchId}", branchId.ToString());
            url = url.Replace("{relaypointId}", relaypointId.ToString());
            var response = await _apiClient.PostAsJsonAsync(url, new { });

            if (!response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                _logger.LogCritical("CatalogApiService AddRelaypointItemMasters Service failure for relaypoint Id:" + relaypointId + " | responseStatus: " + response.StatusCode + " | response: " + serverResponse);
                return false;
            }

            return true;
        }

        public async Task<List<BusinessEntityAssetCount>> GetDealerAssetCountListForDistributor()
        {
            var url = _settings.Value.CatalogApiUrl + _settings.Value.GetDealersAssetCountsForDistributor;
            //url = url.Replace("{distributorId}", distributorId.ToString());
            var response = await _apiClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BusinessEntityAssetCount>>(serverResponse);
            }

            return null;
        }

        public async Task<List<AssetCount>> GetDealerAssetCount(int dealerId)
        {
            var url = _settings.Value.CatalogApiUrl + _settings.Value.GetDealerAssetCounts;
            url = url.Replace("{dealerId}", dealerId.ToString());
            var response = await _apiClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var serverResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AssetCount>>(serverResponse);
            }

            return null;
        }
    }
}

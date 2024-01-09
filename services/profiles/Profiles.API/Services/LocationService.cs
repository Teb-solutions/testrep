using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Profiles.API.Services;
using Microsoft.Extensions.Logging;

namespace EasyGas.Services.Profiles.Services
{
    public class LocationService : ILocationService
    {
        private HttpClient _apiClient;
        private readonly ProfilesDbContext _db;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ILogger _logger;

        public LocationService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ProfilesDbContext ctx, ILogger<LocationService> logger)
        {
            _apiClient = httpClient;
            _apiSettings = apiSettings;
            _db = ctx;
            _logger = logger;
        }

        public async Task<List<VehicleLocationsViewModel>> GetVehicleLocations(int tenantId)
        {
                var url = _apiSettings.Value.LocationsApiUrl + _apiSettings.Value.GetVehicleLocations;
                url += "?token=" + _apiSettings.Value.VehicleLocationsApiAccessToken + "&tenantId=" + tenantId;
                //_apiClient.DefaultRequestHeaders.Accept.Clear();
                var response = await _apiClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<VehicleLocationsViewModel>  vehList = JsonConvert.DeserializeObject<List<VehicleLocationsViewModel>>(responseJson);
                    return vehList;
                }
                else
                {
                    _logger.LogError("LocationService.GetVehicleLocations {response} failure for {url}", responseJson, url);
                }
            
            return null;
        }

        public async Task<VehicleLocationsViewModel> GetVehicleLocation(int vehicleId)
        {
                var url = _apiSettings.Value.LocationsApiUrl + _apiSettings.Value.GetVehicleLocation + vehicleId;
                //_apiClient.DefaultRequestHeaders.Accept.Clear();
                var response = await _apiClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    VehicleLocationsViewModel veh = JsonConvert.DeserializeObject<VehicleLocationsViewModel>(responseJson);
                    return veh;
                }
                else
                {
                    _logger.LogError("LocationService.GetVehicleLocations {response} failure for {url}", responseJson, url);
                }

            return null;
        }



    }
}

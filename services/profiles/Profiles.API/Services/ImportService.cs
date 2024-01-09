using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Profiles.API.ViewModels.Import;
using ImportUser = Profiles.API.ViewModels.Import.User;
using System;
using Vehicle = Profiles.API.ViewModels.Import.Vehicle;

namespace EasyGas.Services.Profiles.Services
{
    public interface IImportService
    {
        Task<List<ImportUser>> GetCustomers();
        Task<List<ImportUser>> GetDistributors();
        Task<List<ImportUser>> GetDrivers();
        Task<List<Vehicle>> GetVehicles();
        Task<List<Pincode>> GetPincodes();
        Task<bool> UpdateWallets(List<WalletUpdate> wallets);
    }

    public class ImportService : IImportService
    {
        private readonly HttpClient _apiClient;
        private readonly ProfilesDbContext _db;
        private readonly IOptions<ApiSettings> _settings;

        public ImportService(ProfilesDbContext db, HttpClient httpClient, IOptions<ApiSettings> settings)
        {
            _db = db;
            _settings = settings;
            _apiClient = httpClient;
        }

        public async Task<List<ImportUser>> GetCustomers()
        {
            try
            {
                var url = _settings.Value.EasyGasPuneProfilesApiBaseUrl + _settings.Value.EasyGasPuneGetCustomersApiUrl;
                url += "?token=ADMINTEST@112233";

                var response = await _apiClient.GetAsync(url);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<ImportUser>>(serverResponse);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<ImportUser>> GetDistributors()
        {
            try
            {
                var url = _settings.Value.EasyGasPuneProfilesApiBaseUrl + _settings.Value.EasyGasPuneGetDistributorsApiUrl;
                url += "?token=ADMINTEST@112233";

                var response = await _apiClient.GetAsync(url);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<ImportUser>>(serverResponse);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<ImportUser>> GetDrivers()
        {
            try
            {
                var url = _settings.Value.EasyGasPuneProfilesApiBaseUrl + _settings.Value.EasyGasPuneGetDriversApiUrl;
                url += "?token=ADMINTEST@112233";

                var response = await _apiClient.GetAsync(url);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<ImportUser>>(serverResponse);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Vehicle>> GetVehicles()
        {
            try
            {
                var url = _settings.Value.EasyGasPuneProfilesApiBaseUrl + _settings.Value.EasyGasPuneGetVehiclesApiUrl;
                url += "?token=ADMINTEST@112233";

                var response = await _apiClient.GetAsync(url);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Vehicle>>(serverResponse);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Pincode>> GetPincodes()
        {
            try
            {
                var url = _settings.Value.EasyGasPuneProfilesApiBaseUrl + _settings.Value.EasyGasPuneGetPincodesApiUrl;
                url += "?token=ADMINTEST@112233";

                var response = await _apiClient.GetAsync(url);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Pincode>>(serverResponse);
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateWallets(List<WalletUpdate> wallets)
        {
            try
            {
                var url = _settings.Value.WalletServiceUrl + _settings.Value.UpdateWalletUniqueIdApiUrl;
                //url += "?token=ADMINTEST@112233";

                var response = await _apiClient.PostAsJsonAsync(url, wallets);
                var serverResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

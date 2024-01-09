using Azure.Storage.Blobs;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Models;
using Profiles.API.ViewModels.AppImage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.BizLogic
{
    public class SettingsMgr
    {
        private ProfilesDbContext _db;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private ILogger<SettingsMgr> _logger;
        public SettingsMgr(ProfilesDbContext db,
           ILogger<SettingsMgr> logger, IOptions<ApiSettings> apiSettings, BlobServiceClient blobServiceClient)
        {
            _db = db;
            _blobServiceClient = blobServiceClient;
            _apiSettings = apiSettings;
            _logger = logger;
        }

        public async Task<CommandResult> AddAppImage(AddAppImageRequest request, int userId)
        {
            AppImage model = new AppImage()
            {
                BranchId = request.BranchId,
                TenantId = request.TenantId,
                Position = request.Position,
                Type = request.Type,
            };

            if (string.IsNullOrEmpty(request.ImageBase64))
            {
                return CommandResult.FromValidationErrors("Invalid image.");
            }

            var imageData = GetImageData(request.ImageBase64);
            if (imageData == null)
            {
                return CommandResult.FromValidationErrors("Invalid image.");
            }
            string fileName = request.Type.ToString() + "_" + Guid.NewGuid().ToString("N") + ".jpg";
            var isSaved = await SaveDataToBlobs(imageData, _apiSettings.Value.BlobCustomerAppImageContainer, fileName);
            if (isSaved)
            {
                model.FileName = fileName;
                _db.UserId = userId.ToString();
                _db.AppImages.Add(model);
                await _db.SaveChangesAsync();
                _logger.LogInformation("App image {imageId} added by {userId}", model.Id, userId);
                return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Image successfully saved."));
            }
            else
            {
                return CommandResult.FromValidationErrors("Image could not be saved. Please try again.");
            }
        }
        public async Task<CommandResult> RemoveAppImage(int id, int userId)
        {
            var appImage = _db.AppImages.Where(p => p.Id == id).FirstOrDefault();
            if (appImage == null)
            {
                return CommandResult.FromValidationErrors("Image not found.");
            }
            var isRemoved = await RemoveDataFromBlobs(_apiSettings.Value.BlobCustomerAppImageContainer, appImage.FileName);
            _db.AppImages.Remove(appImage);
            await _db.SaveChangesAsync();
            _logger.LogInformation("App image {imageId} removed by {userId}", id, userId);
            return new CommandResult(System.Net.HttpStatusCode.OK, new ApiResponse("Image successfully removed."));
        }
        private async Task<bool> SaveDataToBlobs(byte[] imgData, string container, string fileName)
        {
            try
            {
                var blobContainer = _blobServiceClient.GetBlobContainerClient(container);
                await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                var blobClient = blobContainer.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                //await blobClient.UploadAsync(model.ImageFile.OpenReadStream());
                using (var stream = new MemoryStream(imgData))
                {
                    await blobClient.UploadAsync(stream);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private async Task<bool> RemoveDataFromBlobs(string container, string fileName)
        {
            try
            {
                var blobContainer = _blobServiceClient.GetBlobContainerClient(container);
                await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                var blobClient = blobContainer.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static byte[] GetImageData(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}

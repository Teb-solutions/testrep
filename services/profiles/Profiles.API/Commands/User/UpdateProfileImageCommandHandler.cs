using Azure.Storage.Blobs;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateProfileImageCommandHandler : ICommandHandler<UpdateProfileImageCommand>
    {

        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateProfileImageCommandHandler(BlobServiceClient blobServiceClient, IOptions<ApiSettings> apiSettings, ProfilesDbContext db, ILogger<UpdateProfileImageCommandHandler> logger)
        {

            _blobServiceClient = blobServiceClient;
            _apiSettings = apiSettings;
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UpdateProfileImageCommand command)
        {
            var imgData = command.ImageData;
            UserProfile userProfile = _db.Profiles.Include(prop => prop.User).Where(p => p.UserId == command.UserId).FirstOrDefault();
            return SaveDataToBlobs(imgData, userProfile).Result;
        }

        private async Task<CommandHandlerResult> SaveDataToBlobs(byte[] imgData, UserProfile userProfile)
        {
            try
            {
                if (string.IsNullOrEmpty(userProfile.PhotoUrl))
                {
                    // for security reasons
                    if (userProfile.User.Type == Shared.Enums.UserType.CUSTOMER)
                    {
                        userProfile.PhotoUrl = Guid.NewGuid().ToString("N") + "_" + userProfile.UserId + ".jpg";
                    }
                    else
                    {
                        userProfile.PhotoUrl = userProfile.User.Type.ToString() + "_" + userProfile.UserId + ".jpg";
                    }
                }

                var blobContainer = _blobServiceClient.GetBlobContainerClient(_apiSettings.Value.BlobProfileImageContainer);
                await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                var blobClient = blobContainer.GetBlobClient(userProfile.PhotoUrl);

                await blobClient.DeleteIfExistsAsync();

                //await blobClient.UploadAsync(model.ImageFile.OpenReadStream());
                using (var stream = new MemoryStream(imgData))
                {
                    await blobClient.UploadAsync(stream);
                }

                return CommandHandlerResult.OkDelayed(this, x => new Shared.Models.ApiResponse("Profile Image updated successfully"));
            }
            catch (Exception ex)
            {
                return CommandHandlerResult.Error($"Error {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}

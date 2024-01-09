using Azure.Storage.Blobs;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class DeleteProfileImageCommandHandler : ICommandHandler<DeleteProfileImageCommand>
    {
        private readonly ProfilesDbContext _db;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<ApiSettings> _apiSettings;

        public DeleteProfileImageCommandHandler(IConfiguration cfg, ProfilesDbContext db, BlobServiceClient blobServiceClient, IOptions<ApiSettings> apiSettings)
        {
            _blobServiceClient = blobServiceClient;
            _apiSettings = apiSettings;
            _db = db;
        }
        public CommandHandlerResult Handle(DeleteProfileImageCommand command)
        {
            var existing = _db.Profiles.SingleOrDefault(p => p.UserId == command.UserId);
            if (existing != null)
            {
                return DeleteDataFromBlobs(existing).Result;
            }
            else
            {
                return CommandHandlerResult.Error("User not found");
            }
        }

        private async Task<CommandHandlerResult> DeleteDataFromBlobs(UserProfile userProfile)
        {
            try
            {
                if (string.IsNullOrEmpty(userProfile.PhotoUrl))
                {
                    return CommandHandlerResult.Error("Profile Photo not found");
                }
                var blobContainer = _blobServiceClient.GetBlobContainerClient(_apiSettings.Value.BlobProfileImageContainer);

                var blobClient = blobContainer.GetBlobClient(userProfile.PhotoUrl);

                await blobClient.DeleteIfExistsAsync();

                userProfile.PhotoUrl = "";

                return CommandHandlerResult.OkDelayed(this, x => new { message = "Profile Image deleted successfully" });
            }
            catch (Exception ex)
            {
                return CommandHandlerResult.Error($"Some error has occured. Please try again");
            }
        }

        private async Task  DeleteDataFromBlob(string fileName, string containerName)
        {
            
        }
    }
}

using Azure.Storage.Blobs;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UploadInvoiceCommandHandler : ICommandHandler<UploadInvoiceCommand>
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UploadInvoiceCommandHandler(BlobServiceClient blobServiceClient, IOptions<ApiSettings> apiSettings, ProfilesDbContext db, ILogger<UpdateProfileImageCommandHandler> logger)
        {

            _blobServiceClient = blobServiceClient;
            _apiSettings = apiSettings;
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(UploadInvoiceCommand command)
        {
            return SaveDataToBlobs(command._fileData, command._filename).Result;
        }

        private async Task<CommandHandlerResult> SaveDataToBlobs(byte[] data, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                     fileName = Guid.NewGuid().ToString("N") + ".pdf";
                }

                var blobContainer = _blobServiceClient.GetBlobContainerClient(_apiSettings.Value.BlobCustomerInvoicePdfContainer);
                await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                var blobClient = blobContainer.GetBlobClient(fileName);

                await blobClient.DeleteIfExistsAsync();

                //await blobClient.UploadAsync(model.ImageFile.OpenReadStream());
                using (var stream = new MemoryStream(data))
                {
                    await blobClient.UploadAsync(stream);
                }

                return CommandHandlerResult.OkDelayed(this, x => new Shared.Models.ApiResponse("Invoice pdf uploaded successfully"));
            }
            catch (Exception ex)
            {
                return CommandHandlerResult.Error($"Error {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}

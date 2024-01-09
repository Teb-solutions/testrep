using Azure.Storage.Blobs;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Models;
using Profiles.API.ViewModels.Complaint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddComplaintCommandHandler : ICommandHandler<AddComplaintCommand>
    {

        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public AddComplaintCommandHandler(BlobServiceClient blobServiceClient, IOptions<ApiSettings> apiSettings, ProfilesDbContext db, ILogger<AddComplaintCommandHandler> logger)
        {

            _blobServiceClient = blobServiceClient;
            _apiSettings = apiSettings;
            _db = db;
            _logger = logger;
        }

        public CommandHandlerResult Handle(AddComplaintCommand command)
        {
            Complaint model = new Complaint()
            {
                UserId = command.UserId,
                TenantId = 1, //TODO remove hardcode
                Category = command.Model.Category,
                Message = command.Model.Message,
                Subject = command.Model.Subject,
                Status = ComplaintStatus.Open,
                CreatedBy = command.AddedByUserId.ToString()
            };

            return SaveData(model, command.ImageData).Result;
        }

        private async Task<CommandHandlerResult> SaveData(Complaint model, byte[] imgData = null)
        {
            try
            {
                if (imgData != null)
                {
                    model.AttachmentUrl = Guid.NewGuid().ToString("N") + "_" + model.UserId + ".jpg";
                    var blobContainer = _blobServiceClient.GetBlobContainerClient(_apiSettings.Value.BlobCustomerComplaintsImageContainer);
                    await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                    var blobClient = blobContainer.GetBlobClient(model.AttachmentUrl);

                    await blobClient.DeleteIfExistsAsync();

                    //await blobClient.UploadAsync(model.ImageFile.OpenReadStream());
                    using (var stream = new MemoryStream(imgData))
                    {
                        await blobClient.UploadAsync(stream);
                    }
                }
                
                _db.Complaints.Add(model);
                return CommandHandlerResult.OkDelayed(this, x => new CreateComplaintResponse { Id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"AddTicketCommand error {ex.Message}");
                return CommandHandlerResult.Error($"Some error has occured.");
            }
        }

    }
}

using EasyGas.Services.Core.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UploadExcelCommandHandler : ICommandHandler<UploadExcelCommand>
    {
        private readonly string _blobConnectionString;
        public UploadExcelCommandHandler(IConfiguration cfg)
        {
            _blobConnectionString = cfg["ConnectionStrings:avatarBlob"];
        }
        public CommandHandlerResult Handle(UploadExcelCommand command)
        {
            var filename = command._filename;
            return SaveDataToBlobs(filename).Result;
        }
        private async Task<CommandHandlerResult> SaveDataToBlobs(string fileName)
        {
            try
            {
                await SaveDataToBlob(fileName, "report-input");
                return CommandHandlerResult.Ok;
            }
            catch (Exception ex)
            {
                return CommandHandlerResult.Error($"Error {ex.GetType().Name} - {ex.Message}");
            }
        }

        private async Task SaveDataToBlob(string fileName, string containerName)
        {
            /*
            var storageAccount = CloudStorageAccount.Parse(_blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.UploadFromFileAsync("exl\\" + fileName);
            */
        }

      
    }
}

using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UploadInvoiceCommand:CommandBase
    {
        public string _filename { get; set; }
        public byte[] _fileData { get; set; }
        public string _fileMimeType { get; set; }

        public UploadInvoiceCommand(string filename, byte[] fileData, string fileMimeType)
        {
            _filename = filename;
            _fileData = fileData;
            _fileMimeType = fileMimeType;
        }
        protected override IEnumerable<string> OnValidation()
        {
            if (string.IsNullOrEmpty(_filename))
            {
                yield return "Payload not found or payload data is empty";
            }
        }

    }
}

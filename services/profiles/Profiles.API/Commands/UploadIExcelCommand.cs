using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class UploadExcelCommand:CommandBase

    {
        public string _filename { get; set; }

        public UploadExcelCommand(string filename)
        {
            _filename = filename;
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

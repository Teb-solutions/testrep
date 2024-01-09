using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Complaint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class AddComplaintCommand : CommandBase
    {

        public NewCustomerComplaint Model { get; }

        public byte[] ImageData { get; private set; }
        public int UserId { get; set; }
        public int AddedByUserId { get; set; }

        public AddComplaintCommand(int userid, int addedByuserId, NewCustomerComplaint model)
        {
            Model = model;
            UserId = userid;
            AddedByUserId = addedByuserId;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Model == null)
            {
                yield return "Data is empty";
            }
            else
            {
                if (string.IsNullOrEmpty(Model.Subject))
                {
                    yield return "Subject is empty";
                }
                if (string.IsNullOrEmpty(Model.Message))
                {
                    yield return "Message is empty";
                }
                if (UserId <= 0 )
                {
                    yield return "Customer is invalid";
                }
                if (!string.IsNullOrEmpty(Model.AttachmentBase64))
                {
                    ImageData = GetImageData(Model.AttachmentBase64);
                    if (ImageData == null)
                    {
                        yield return "Attachment is invalid";
                    }
                }
                
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

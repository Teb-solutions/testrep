using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models.MessageRequest
{
    public class RequestModel
    {
        public string RecipientName { get; set; }
        public string RecipientNumber { get; set; }
        public string RecipientEmail { get; set; }
        public string CC { get; set; }
        public string MessageHeader { get; set; }
        public MessageType Type { get; set; }
        public object content { get; set; }

        public List<string> AttachmentUrls;
    }
    public enum MessageType
    {
        SMS,
        VOICE,
        EMAIL
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models.EInvoice
{
    public class EInvTokenRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }

        public string grant_type { get; set; }
    }

    public class EInvTokenSuccessResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class EInvTokenErrorResponse
    {
        public string error { get; set; }
        public int error_description { get; set; }
    }
}

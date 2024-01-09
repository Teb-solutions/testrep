using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class CreateInvoiceResponse
    {
        public string InvoicePdfUrl { get; set; }
        public string InvoiceHtmlString { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}

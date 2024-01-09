using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class InvoiceModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual User Customer { get; set; }
        public int? OrderId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string PONumber { get; set; }
        public DateTime PODate { get; set; }
        public string EWayBillNumber { get; set; }
        public int? DistributorId { get; set; }
        public string PdfLink { get; set; }
        public string PdfHtmlString { get; set; }
    }
}

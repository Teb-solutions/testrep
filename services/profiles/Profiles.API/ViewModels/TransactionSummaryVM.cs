using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class TransactionSummaryVM
    {
        public int DistributorId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<OrderSummaryVM> Orders { get; set; }
        public int TotalDeliveredOrders { get; set; }
        public double TotalAmount { get; set; }
    }

    public class OrderSummaryVM
    {
        public int SlNo { get; set; }
        public string InvoiceNumber { get; set; }
        [DisplayName("Date")]
        public string DeliveredDate { get; set; }

        [DisplayName("CustomerName")]
        public string CustomerName { get; set; }

        [DisplayName("Customer Address")]
        public string Address { get; set; }

        [DisplayName("Driver Name")]
        public string DriverName { get; set; }

        public string OrderType { get; set; }
        [DisplayName("Taxable Amount")]
        public double TaxableAmount { get; set; }

        [DisplayName("Tax")]
        public double Tax { get; set; }

        [DisplayName("Total Amount")]
        public double TotalAmount { get; set; }
        public string Invoices { get; set; }
    }
}

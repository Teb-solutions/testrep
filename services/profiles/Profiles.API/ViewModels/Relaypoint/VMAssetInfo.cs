using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RelayPointLogistics.Models
{
    public class VMAssetInfo
    {

        public int AssetId { get; set; }


        public int UserId { get; set; }


        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public string ItemCode { get; set; }
        
       
        //public int CategoryID { get; set; }
        //public int? LastTransactedItem { get; set; }
        //[ForeignKey("LastTransactedItem")]

        //public virtual Booking booking{ get; set; }

        // public int TransactionUnits { get; set; }

        public int BlockedtoSell { get; set; }

        public int BlockedtoBuy { get; set; }

        public int AvailableToSell { get; set; }

        //[Required]
        // [MaxLength(50)]
        // public string Name { get; set; }



        // public decimal ListPrice { get; set; }
        // [Required]
        // public decimal RelayPointPrice { get; set; }

        public int? TaxType { get; set; }
        public DateTimeOffset? DiscountStartDate { get; set; }
        public DateTimeOffset? DiscountEndDate { get; set; }

        
        public int CurrentStockUnits { get; set; }

    }
}

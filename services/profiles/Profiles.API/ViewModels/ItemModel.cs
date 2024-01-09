using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Models
{
    public class ItemModel
    {
        public int ItemId { get; set; }
        public int? OrderItemId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string HSN { get; set; }
        public string ProductDescription { get; set; }
        public string Specification { get; set; }
       // public ItemCategory Category { get; set; }
        //public ItemType Type { get; set; }
        //public OrderType? OrderType { get; set; }
        public SlotType? SlotType { get; set; }
        public ItemPriceDate PriceDate { get; set; }
        public int Quantity { get; set; }
        public float TotalAmount { get; set; }
        public int DisplayOrder { get; set; }

    }

    public class ItemPriceDate
    {
        public int ItemId { get; set; }
        public float Price { get; set; }
        public DateTime? FromDate { get; set; }
    }
}

using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RelayPointLogistics.Models
{


    public class VMRelayModel
    {

        public int? Id { get; set; }
        public string Code { get; set; }

        public string RetailShopName { get; set; }


        public UserAndProfileModel UserProfile { get; set; }

        public AddressModel Address { get; set; }



        public DateTime? StartTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public int Ratings { get; set; }

        public List<ItemModel> ItemList { get; set; }

        public string MobileNumber { get; set; }

        public string CreatedBy { get; set; }

        //public RetailPointStatus Status{ get; set; }

        public int StockCount { get; set; }

        public int StockThreshold { get; set; }

        public int EmptyStockCount { get; set; }


    }

}


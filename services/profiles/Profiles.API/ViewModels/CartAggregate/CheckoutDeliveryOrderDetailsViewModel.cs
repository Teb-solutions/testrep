using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.CartAggregate
{
    public class CheckoutDeliveryOrderDetailsViewModel
    {
        public DeliverySlotModel DeliverySlot {get; set;}
        public AddressModel DeliveryAddress { get; set; }
        public CustomerProfileModel Profile { get; set; }

    }
}

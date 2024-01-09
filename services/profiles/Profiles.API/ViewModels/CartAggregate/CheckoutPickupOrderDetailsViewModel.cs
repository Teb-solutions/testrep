using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.CartAggregate
{
    public class CheckoutPickupOrderDetailsViewModel
    {
        public BusinessEntityModel Relaypoint { get; set;}
        public CustomerProfileModel Profile { get; set; }
    }
}

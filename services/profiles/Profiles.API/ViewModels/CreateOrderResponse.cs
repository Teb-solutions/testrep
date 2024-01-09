using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Models
{
    public class CreateOrderResponse
    {
        public int userId { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public int OrderAdressId { get; set; }
        public int UserAddressId { get; set; }
        public SlotType DeliverySlotType { get; set; }
        public bool IsNewUser { get; set; }
    }

    public class CreateProfileResponse
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
    }

    public class CreateUserAddressResponse
    {
        public int UserAddressId { get; set; }
        public int BranchId { get; set; }
    }

}

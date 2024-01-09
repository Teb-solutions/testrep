using System.Collections.Generic;

namespace EasyGas.Services.Profiles.Models
{
    public class PWCreateOrderVM
    {
        public IEnumerable<DeliverySlotModel> DeliverySlotList { get; set; }
    }
}

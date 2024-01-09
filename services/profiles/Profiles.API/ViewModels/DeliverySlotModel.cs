using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Models
{
    public class DeliverySlotModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SlotType Type { get; set; }
        public DateTime? From { get; set; }
        public int FromSec { get; set; }
        public DateTime? To { get; set; }
        public int ToSec { get; set; }
        public int MaxThreshold { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeliverySlotWithDate
    {
        public int SlotId { get; set; }
        public DateTime Date { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int OrdersCount { get; set; }
        public int SlotThreshold { get; set; }
        public bool IsActive { get; set; }
        public string DateName { get; set; }
        public string SlotName { get; set; }
        public SlotType SlotType { get; set; }
        public string UID { get; set; }
    }
}

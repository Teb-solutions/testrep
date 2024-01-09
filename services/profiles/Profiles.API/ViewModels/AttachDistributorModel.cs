using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class AttachDistributorModel
    {
        public int UserId { get; set; }
        public int DistributorId { get; set; }
    }

    public class AttachDistributorToOrderRequest
    {
        public int AttachedBusinessEntityId { get; set; }
        public string AttachedBusinessEntityName { get; set; }
    }
}

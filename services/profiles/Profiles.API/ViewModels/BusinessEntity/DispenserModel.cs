using System.Collections.Generic;

namespace Profiles.API.ViewModels.BusinessEntity
{
    public class DispenserModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? DeviceId { get; set; }
        public string? SecretKey { get; set; }
        public bool IsActive { get; set; }
        public List<NozzleModel> Nozzles { get; set; }
    }
}

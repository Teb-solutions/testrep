using EasyGas.Services.Profiles.Models;
using EasyGas.Shared;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Relaypoint;
using System.Collections.Generic;

namespace Profiles.API.ViewModels.BusinessEntity
{
    public class CreateDispenserRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? DeviceId { get; set; }
        public string? SecretKey { get; set; }
        public bool IsActive { get; set; }
        public List<NozzleModel> Nozzles { get; set; }
    }

    public class NozzleModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? DeviceId { get; set; }
        public FuelType? FuelType { get; set; }
        public bool IsActive { get; set; }
    }
}

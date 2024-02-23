using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profiles.API.Controllers;
using Profiles.API.IntegrationEvents;
using Profiles.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/v1/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class DeliverySlotController : BaseApiController
    {
        private readonly ProfilesDbContext _db;
        private readonly ICartService _cartService;
        private readonly IProfilesIntegrationEventService _profilesIntegrationEventService;
        public DeliverySlotController(ProfilesDbContext db, IProfilesIntegrationEventService profilesIntegrationEventService,
            ICartService cartService, ICommandBus bus)
            : base(bus)
        {
            _db = db;
            _cartService = cartService;
            _profilesIntegrationEventService = profilesIntegrationEventService;
        }

        [Route("branch/{branchId}")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DeliverySlotModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetListByBranchId(int branchId)
        {
            var slots = await GetDeliverySlotsByBranchId(branchId);
            return Ok(slots);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DeliverySlotModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetList()
        {
            var slots = await _db.DeliverySlots.Include(p => p.Branch)
            .Select(p => new DeliverySlotModel()
            {
                Id = p.Id,
                UID = p.UID,
                Description = p.Description,
                From = p.From,
                Price = p.Price,
                FromSec = p.FromSec,
                Name = p.Name,
                TenantId = p.TenantId,
                BranchId = p.BranchId,
                BranchName = p.Branch.Name,
                To = p.To,
                ToSec = p.ToSec,
                MaxThreshold = p.MaxThreshold,
                Type = p.Type,
                IsActive = p.IsActive
            }).ToListAsync();
            return Ok(slots);
        }

        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(DeliverySlotModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var slot = _db.DeliverySlots.Include(p => p.Branch).Where(p => p.Id == id).FirstOrDefault();
            if (slot == null)
            {
                return NotFound();
            }

            var slotModel = new DeliverySlotModel()
            {
                Id = slot.Id,
                UID = slot.UID,
                Description = slot.Description,
                From = slot.From,
                Price = slot.Price,
                FromSec = slot.FromSec,
                Name = slot.Name,
                TenantId = slot.TenantId,
                BranchId = slot.BranchId,
                BranchName = slot.Branch.Name,
                To = slot.To,
                ToSec = slot.ToSec,
                MaxThreshold = slot.MaxThreshold,
                Type = slot.Type,
                IsActive = slot.IsActive
            };
            return Ok(slotModel);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeliverySlotModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody] DeliverySlotModel slot)
        {
            List<string> validationErrors = new List<string>();

            var branch = await _db.Branches.FindAsync(slot.BranchId);
            if (branch == null)
            {
                validationErrors.Add("Branch is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            DeliverySlot slotEntity = new DeliverySlot()
                {
                    UID = slot.UID,
                    Description = slot.Description,
                    From = slot.From,
                    Price = slot.Price,
                    FromSec = slot.FromSec,
                    Name = slot.Name,
                    TenantId = slot.TenantId,
                    BranchId = slot.BranchId,
                    To = slot.To,
                    ToSec = slot.ToSec,
                    MaxThreshold = slot.MaxThreshold,
                    Type = slot.Type,
                    IsActive = slot.IsActive
                };
                _db.Add(slotEntity);
                
            await _db.SaveChangesAsync();
            slot.Id = slotEntity.Id;

            var slots = await GetDeliverySlotsByBranchId(slot.BranchId, true);
            await _cartService.UpdateDeliverySlots(slot.BranchId, slots);

            return Ok(slot);
        }

        [HttpPut]
        [ProducesResponseType(typeof(DeliverySlotModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update([FromBody] DeliverySlotModel slot)
        {
            List<string> validationErrors = new List<string>();

            var branch = await _db.Branches.FindAsync(slot.BranchId);
            if (branch == null)
            {
                validationErrors.Add("Branch is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            var slotEntity = await _db.DeliverySlots.FindAsync(slot.Id);
            if (slotEntity == null)
            {
                validationErrors.Add("Slot is invalid");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }

            decimal oldPrice = slotEntity.Price;

            slotEntity.UID = slot.UID;
            slotEntity.Description = slot.Description;
            slotEntity.From = slot.From;
            slotEntity.Price = slot.Price;
            slotEntity.FromSec = slot.FromSec;
            slotEntity.Name = slot.Name;
            slotEntity.TenantId = slot.TenantId;
            slotEntity.BranchId = slot.BranchId;
            slotEntity.To = slot.To;
            slotEntity.ToSec = slot.ToSec;
            slotEntity.MaxThreshold = slot.MaxThreshold;
            slotEntity.Type = slot.Type;
            slotEntity.IsActive = slot.IsActive;
            _db.Entry(slotEntity).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            var slots = await GetDeliverySlotsByBranchId(slot.BranchId, true);
            await _cartService.UpdateDeliverySlots(slot.BranchId, slots);

            if (oldPrice != slotEntity.Price)
            {
                DeliverySlotPriceChangedIntegrationEvent @event = new DeliverySlotPriceChangedIntegrationEvent()
                {
                    BranchId = slotEntity.BranchId,
                    DeliverySlotId = slotEntity.Id,
                    IsSlotExpress = slotEntity.Type == Shared.Enums.SlotType.SLOT_EXPRESS,
                    NewPrice = slotEntity.Price
                };
                //await _profilesIntegrationEventService.PublishEventThroughEventBusAsync(@event);
            }

            return Ok(slot);
        }

        private async Task<List<DeliverySlotModel>> GetDeliverySlotsByBranchId(int branchId, bool? isActive = null)
        {
            var slotsQuery = _db.DeliverySlots
                .Include(p => p.Branch)
                .Where(p => p.BranchId == branchId && !p.IsDeleted)
                .AsQueryable();

            if (isActive != null)
            {
                slotsQuery = slotsQuery.Where(p => p.IsActive == isActive);
            }
                 
            var slots = await slotsQuery
                .Select(p => new DeliverySlotModel()
                    {
                        Id = p.Id,
                        UID = p.UID,
                        Description = p.Description,
                        From = p.From,
                        Price = p.Price,
                        FromSec = p.FromSec,
                        Name = p.Name,
                        TenantId = p.TenantId,
                        BranchId = p.BranchId,
                        BranchName = p.Branch.Name,
                        To = p.To,
                        ToSec = p.ToSec,
                        MaxThreshold = p.MaxThreshold,
                        Type = p.Type,
                        IsActive = p.IsActive
                    }).ToListAsync();
            return slots;
        }
    }
}

using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profiles.API.Controllers;
using Profiles.API.ViewModels.Pincode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/v1/[controller]")]
    public class PincodeController : BaseApiController
    {
        private readonly ProfilesDbContext _db;

        public PincodeController(ProfilesDbContext db, ICommandBus bus)
            : base(bus)
        {
            _db = db;
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PincodeModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetList([FromQuery] int tenantId, [FromQuery] int? branchId)
        {
                var query = _db.Pincodes.Include(pincodes => pincodes.Branch).AsQueryable();
                if (tenantId != null)
                {
                    query = query.Where(p => p.TenantId == tenantId);
                }
                if (branchId != null)
                {
                    query = query.Where(p => p.BranchId == branchId);
                }
                    var pincodes = await query.Select(p => new PincodeModel() 
                    { 
                        Id = p.Id,
                        Code = p.Code,
                        BranchId = p.BranchId,
                        BranchName = p.Branch.Name,
                        IsActive = p.IsActive,
                        UpdatedAt = p.UpdatedAt,
                        UpdatedBy = p.UpdatedBy
                    }).ToListAsync();

                    return Ok(pincodes);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(PincodeModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var pincode = _db.Pincodes.Include(pincodes => pincodes.Branch).Where(p => p.Id == id).FirstOrDefault();
                if (pincode != null)
                {
                    var pincodeModel = new PincodeModel()
                    {
                        Id = pincode.Id,
                        Code = pincode.Code,
                        BranchId = pincode.BranchId,
                        BranchName = pincode.Branch.Name,
                        IsActive = pincode.IsActive,
                        UpdatedAt = pincode.UpdatedAt,
                        UpdatedBy = pincode.UpdatedBy
                    };
                    return Ok(pincodeModel);
                }
                return NotFound();    
            }
            catch (Exception ex)
            {

            }
            return BadRequest();
        }

        [Authorize(Roles = "TENANT_ADMIN")]
        [HttpPost]
        [ProducesResponseType(typeof(PincodeModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody] CreatePincodeRequest request)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                Pincode existing = _db.Pincodes.Where(p => p.Code == request.Code).FirstOrDefault();
                if (existing != null)
                {
                   validationErrors.Add("Pincode already exists.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                Pincode newPincode = new Pincode()
                    {
                    TenantId = request.TenantId,
                    BranchId = request.BranchId,
                        Code = request.Code,
                        IsActive = true,
                    };
                _db.UserId = request.CreatedBy;
                    _db.Add(newPincode);
                
                await _db.SaveChangesAsync();
                return Ok(request);
            }
            catch(Exception ex)
            {

            }
            validationErrors.Add("Some error has occurred. Please try again.");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        [Authorize(Roles = "TENANT_ADMIN")]
        [Route("disable")]
        [HttpPut]
        public async Task<IActionResult> Disable([FromBody] EnablePincodeRequest request)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var pincodeRow = _db.Pincodes.Where(predicate => predicate.Id == request.Id).FirstOrDefault();
                if (pincodeRow == null)
                {
                    validationErrors.Add("Pincode does not exist.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                pincodeRow.IsActive = false;
                pincodeRow.DisabledBy = request.UpdatedBy;
                pincodeRow.UpdatedBy = request.UpdatedBy;
                _db.UserId = request.UpdatedBy;
                _db.Entry(pincodeRow).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {

            }
            validationErrors.Add("Some error has occurred. Please try again.");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        [Authorize(Roles = "TENANT_ADMIN")]
        [Route("enable")]
        [HttpPut]
        public async Task<IActionResult> Enable([FromBody] EnablePincodeRequest request)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                var pincodeRow = _db.Pincodes.Where(predicate => predicate.Id == request.Id).FirstOrDefault();
                if (pincodeRow == null)
                {
                    validationErrors.Add("Pincode does not exist.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                pincodeRow.IsActive = true;
                pincodeRow.EnabledBy = request.UpdatedBy;
                pincodeRow.UpdatedBy = request.UpdatedBy;
                _db.UserId = request.UpdatedBy;
                _db.Entry(pincodeRow).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {

            }
            validationErrors.Add("Some error has occurred. Please try again.");
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }
    }
}

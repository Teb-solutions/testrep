using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Infrastructure.Services;
using Profiles.API.Models;
using Profiles.API.ViewModels.Account;
using Profiles.API.ViewModels.AdminWebsiteVM;
using Profiles.API.ViewModels.BusinessEntity;
using Profiles.API.ViewModels.Complaint;
using Profiles.API.ViewModels.Crm;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Profiles.API.Controllers
{
    [Route("profiles/api/v1/[controller]")]
    [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE, CUSTOMER_CARE_ADMIN")]
    public class CrmController : BaseApiController
    {
        private readonly ILogger<CrmController> _logger;
        private readonly IIdentityService _identityService;
        private readonly ProfilesDbContext _db;
        private readonly IBusinessEntityQueries _businessEntityQueries;
        private readonly CrmMgr _crmMgr;
        private readonly WalletMgr _walletMgr;
        private readonly IOptions<ApiSettings> _apiSettings;

        public CrmController(ILogger<CrmController> logger, ICommandBus bus, 
            IIdentityService identityService, ProfilesDbContext db, CrmMgr crmMgr,
            WalletMgr walletMgr, IOptions<ApiSettings> apiSettings, IBusinessEntityQueries businessEntityQueries)
            : base(bus)
        {
            _logger = logger;
            _identityService = identityService;
            _db = db;
            _crmMgr = crmMgr;
            _walletMgr = walletMgr;
            _apiSettings = apiSettings;
            _businessEntityQueries = businessEntityQueries;
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordModel data)
        {
            LoginModel loginModel = new LoginModel
            {
                GrantType = LoginModel.PasswordGrantType,
                Credentials = data.Password,
                UserType = UserType.CUSTOMER_CARE,
                UserName = data.UserName,
                DeviceId = data.DeviceId,
                Source = EasyGas.Shared.Source.CRM
            };

            return await _identityService.Authenticate(loginModel, GetIpAddress());
        }

        //[Authorize(Roles = "TENANT_ADMIN, CUSTOMER_CARE_ADMIN")]
        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register([FromBody] CreateCrmStaffRequest request)
        {
            return await _crmMgr.Register(request);
        }

        #region businessentity

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentity/create")]
        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCreateBusinessEntityLists()
        {
            var branches = await _db.Branches.Select(p => new { p.Id, p.Name}).ToListAsync();
            var businessEntityTypes = new List<object>
            {
                new { Id = BusinessEntityType.Distributor, Name = "Distributor"},
                new { Id = BusinessEntityType.Dealer, Name = "Dealer"},
                new { Id = BusinessEntityType.Alds, Name = "Alds"},
                new { Id = BusinessEntityType.CarWash, Name = "Car Wash"},
                new { Id = BusinessEntityType.Lubs, Name = "Lubs"},
            };

            return Ok(new { branches, businessEntityTypes });
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentitylist")]
        [HttpGet]
        [ProducesResponseType(typeof(List<BusinessEntityModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBusinessEntityList()
        {
            var businessEntityList = await _businessEntityQueries.GetAllList();
            return Ok(businessEntityList);
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentity/parents/{type}")]
        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetParentBusinessEntityList(BusinessEntityType type)
        {
            if (type == BusinessEntityType.Dealer)
            {
               var parentBusinessEntityList = await _db.BusinessEntities.Where(p => p.Type == BusinessEntityType.Distributor || p.Type == BusinessEntityType.Dealer)
                    .Select(p => new
                    {
                        p.Id, p.Name, p.Type
                    }).
                    ToListAsync();

                return Ok(parentBusinessEntityList);
            }
            else
            {
                var parentBusinessEntityList = await _db.BusinessEntities.Where(p => p.Type == type)
                     .Select(p => new
                     {
                         p.Id,
                         p.Name,
                         p.Type
                     }).
                     ToListAsync();

                return Ok(parentBusinessEntityList);
            }
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentity/create")]
        [HttpPost]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateBusinessEntity([FromBody] CreateBusinessEntityRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;

            var command = new CreateBusinessEntityCommand(request);
            var commandResult = ProcessCommand(command);

            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (!result.IsOk)
                {
                    return commandResult;
                }

                var createDistributorResponse = result.Content as CreateDistributorResponse;

                // TODO convert to event publish
                await _walletMgr.CreateBusinessEntityWallet(createDistributorResponse.Id);
                return commandResult;
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentity/update/{id}")]
        [HttpPut]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateBusinessEntity(int id, [FromBody] CreateBusinessEntityRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;
            request.Id = id;

            var command = new UpdateBusinessEntityCommand(request, false, false);
            var commandResult = ProcessCommand(command);
            return commandResult;
        }

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("businessentity/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(CreateBusinessEntityRequest), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBusinessEntityByIdForUpdate(int id)
        {
            var businessEntity = await _businessEntityQueries.GetDetailsByIdForUpdate(id);
            if (businessEntity == null) return NotFound();
            return Ok(businessEntity);
        }

        /*
        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, CUSTOMER_CARE_ADMIN")]
        [Route("create")]
        [HttpPost]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateBusinessEntity([FromBody] CreateBusinessEntityRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;

            var command = new CreateBusinessEntityCommand(request);
            var commandResult = ProcessCommand(command);

            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (!result.IsOk)
                {
                    return commandResult;
                }

                var createDistributorResponse = result.Content as CreateDistributorResponse;

                // TODO convert to event publish
                
                await _walletMgr.CreateBusinessEntityWallet(createDistributorResponse.Id);
                return commandResult;
            }

            return CommandResult.FromValidationErrors("Some error has occurred. Please try again.");
        }
        */

        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN")]
        [Route("update")]
        [HttpPut]
        [ProducesResponseType(typeof(CreateDistributorResponse), (int)HttpStatusCode.OK)]
        public IActionResult Update([FromBody] CreateDistributorRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            request.UpdatedBy = userId;

            var command = new UpdateDistributorCommand(request);
            return ProcessCommand(command);
        }

        /*
        [Authorize(Roles = "TENANT_ADMIN, BRANCH_ADMIN, VITE_ADMIN, CUSTOMER_CARE")]
        [Route("indexpage")]
        [HttpGet()]
        [ProducesResponseType(typeof(DistributorIndexpageModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] int tenantId, [FromQuery] int? branchId)
        {
            var list = await _businessEntityQueries.GetAllList(BusinessEntityType.Distributor, null, tenantId, branchId, null, null, null);
            DistributorIndexpageModel response = new DistributorIndexpageModel()
            {
                DistributorList = list
            };
            return Ok(response);
        }
        */

        #endregion

        #region Complaints
        /*
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CrmTicketModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId, [FromQuery] int? branchId)
        {
            //int userId = _identityService.GetUserIdentity();

            var complaintQuery = _db.Complaints.Where(p => !p.IsDeleted).AsQueryable();

            if (tenantId != null)
            {
                complaintQuery = complaintQuery.Where(p => p.TenantId == tenantId);
            }

            if (branchId != null)
            {
                complaintQuery = complaintQuery.Where(p => p.BranchId == branchId);
            }

            var data = await complaintQuery
                .Include(p => p.User)
                .ThenInclude(p => p.Profile)
                .Include(p => p.ClosedByUser)
                .ThenInclude(p => p.Profile)
                .ToListAsync();

            List<CrmTicketModel> complaintList = new List<CrmTicketModel>();
            var storageUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerComplaintsImageContainer;
            foreach (var complaint in data)
            {
                CrmTicketModel complaintModel = CrmTicketModel.FromComplaint(complaint, storageUrl);
                complaintList.Add(complaintModel);
            }

            return Ok(complaintList);
        }

       
        [Route("{complaintId:int}")]
        [HttpDelete]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(int complaintId)
        {
            int userId = _identityService.GetUserIdentity();

            var ticket = _db.Complaints.SingleOrDefault(p => p.Id == complaintId);
            if (ticket == null)
            {
                return BadRequest("Invalid Ticket");
            }

            ticket.IsDeleted = true;
            _db.UserId = userId.ToString();
            _db.Entry(ticket).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok("Ticket deleted successfully");
        }

        [Route("remarks/{ticketId:int}")]
        [HttpPut]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddRemarks(int ticketId, [FromBody] CloseCrmTicketRequest request)
        {
            int userId = _identityService.GetUserIdentity();

            var ticket = _db.Complaints.SingleOrDefault(p => p.Id == ticketId);
            if (ticket == null)
            {
                return BadRequest("Invalid Ticket");
            }
            if (ticket.Status == ComplaintStatus.Closed)
            {
                return BadRequest("Ticket already closed");
            }

            ticket.Remarks = request.Remarks;

            _db.UserId = userId.ToString();
            _db.Entry(ticket).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok("Ticket remarks updated successfully");
        }

        [Route("close/{ticketId:int}")]
        [HttpPut]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CloseTicket(int ticketId, [FromBody] CloseCrmTicketRequest request)
        {
            int userId = _identityService.GetUserIdentity();

            var ticket = _db.Complaints.SingleOrDefault(p => p.Id == ticketId);
            if (ticket == null)
            {
                return BadRequest("Invalid Ticket");
            }
            if (ticket.Status == ComplaintStatus.Closed)
            {
                return BadRequest("Ticket already closed");
            }

            ticket.Remarks = request.Remarks;
            ticket.Status = ComplaintStatus.Closed;
            ticket.ClosedAt = DateMgr.GetCurrentIndiaTime();
            ticket.ClosedByUserId = userId;

            _db.UserId = userId.ToString();
            _db.Entry(ticket).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok("Ticket closed successfully");
        }

        [Route("reopen/{ticketId:int}")]
        [HttpPut]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ReopenTicket(int ticketId, [FromBody] ReopenCrmTicketRequest request)
        {
            int userId = _identityService.GetUserIdentity();

            var ticket = _db.Complaints.SingleOrDefault(p => p.Id == ticketId);
            if (ticket == null)
            {
                return BadRequest("Invalid Ticket");
            }
            if (ticket.Status == ComplaintStatus.Open || ticket.Status == ComplaintStatus.ReOpened)
            {
                return BadRequest("Ticket already in opened state");
            }

            ticket.ReOpenReason = request.Reason;
            ticket.Status = ComplaintStatus.ReOpened;
            ticket.ReOpenAt = DateMgr.GetCurrentIndiaTime();
            ticket.ReOpenByUserId = userId;

            _db.UserId = userId.ToString();
            _db.Entry(ticket).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok("Ticket reopened successfully");
        }
        */
        #endregion
    }
}

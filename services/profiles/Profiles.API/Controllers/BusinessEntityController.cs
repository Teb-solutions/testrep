using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Configuration;
using EasyGas.Services.Profiles.Models.AdminWebsiteVM;
using Microsoft.AspNetCore.Authorization;
using EasyGas.Services.Profiles.BizLogic;
using Profiles.API.Controllers;
using EasyGas.Shared.Enums;
using Profiles.API.ViewModels.Distributor;
using Profiles.API.Infrastructure.Services;
using Profiles.API.ViewModels.AdminWebsiteVM;
using System.Net;
using Profiles.API.ViewModels.Relaypoint;
using EasyGas.Shared;
using System.Collections.Generic;
using System.Linq;
using Profiles.API.Services;
using Profiles.API.Models;
using EasyGas.Shared.Formatters;
using Profiles.API.ViewModels.Staff;
using Profiles.API.ViewModels.BusinessEntity;
using EasyGas.Services.Profiles.Data;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Device = EasyGas.Services.Profiles.Models.Device;
using Profiles.API.ViewModels.Import;
using VehicleState = EasyGas.Services.Profiles.Models.VehicleState;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "DISTRIBUTOR_ADMIN, DEALER_ADMIN, ALDS_ADMIN")]
    public class BusinessEntityController : BaseApiController
    {
        private readonly IProfileQueries _queries;
        private readonly IVehicleQueries _vehQueries;
        private readonly IBusinessEntityQueries _businessEntityQueries;
        private WalletMgr _walletMgr;
        private ProfileMgr _profileMgr;
        private readonly ILocationService _locationService;
        private readonly ICatalogService _catalogService;
        private readonly IIdentityService _identityService;
        private readonly ProfilesDbContext _db;

        public BusinessEntityController(IProfileQueries queries, WalletMgr walletMgr,
            IBusinessEntityQueries businessEntityQueries,
            IVehicleQueries vehQueries,
            IIdentityService identityService,
            ICatalogService catalogService,
            ProfileMgr profileMgr,
            ILocationService locationService,
            ProfilesDbContext db,
            IConfiguration cfg, ICommandBus bus)
            : base(bus)
        {
            _db = db;
            _queries = queries;
            _businessEntityQueries = businessEntityQueries;
            _vehQueries = vehQueries;
            _identityService = identityService;
            _walletMgr = walletMgr;
            _profileMgr = profileMgr;
            _locationService = locationService;
            _catalogService = catalogService;
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordModel data, [FromHeader(Name = "x-clientapp")] string clientapp)
        {
            if (!ModelState.IsValid)
            {
                return CommandResult.FromValidationErrors("Username/password is invalid");
            }

            Source? requestSource = null;
            if (!string.IsNullOrEmpty(clientapp))
            {
                requestSource = Helper.GetSourceFromHeader(clientapp);
            }

            LoginModel loginModel = new LoginModel()
            {
                UserName = data.UserName,
                Credentials = data.Password,
                DeviceId = data.DeviceId,
                GrantType = LoginModel.PasswordGrantType,
                Source = requestSource,
            };
            return await _identityService.Authenticate(loginModel, GetIpAddress());
        }

        [Route("profile")]
        [HttpGet()]
        [ProducesResponseType(typeof(DistributorProfile), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProfile()
        {
            int entityId = _identityService.GetBusinessEntityId();
            var response = await _businessEntityQueries.GetProfile(entityId);
            if (response == null)
                return NotFound("Entity not found");
            return Ok(response);
        }

        [Route("profile")]
        [HttpPut]
        [ProducesResponseType(typeof(DistributorProfile), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDistributorProfile request)
        {
            int entityId = _identityService.GetBusinessEntityId();
            return await _profileMgr.UpdateBusinessEntityProfile(entityId, request);
        }

        [Route("address")]
        [HttpGet()]
        [ProducesResponseType(typeof(UpdateDistributorAddress), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAddress()
        {
            int entityId = _identityService.GetBusinessEntityId();
            var response = await _businessEntityQueries.GetAddress(entityId);
            if (response == null)
                return NotFound("Entity not found");
            return Ok(response);
        }

        [Route("address")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateDistributorAddress request)
        {
            int entityId = _identityService.GetBusinessEntityId();
            return await _profileMgr.UpdateBusinessEntityAddress(entityId, request);
        }

        [Route("drivers")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DistributorDriverModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDriverList()
        {
            int entityId = _identityService.GetBusinessEntityId();
            var drivers = (await _queries.GetDriverList(null, null, entityId))
                .Select(p => new DistributorDriverModel()
                {
                    Id = p.UserProfile.UserId,
                    Email = p.UserProfile.Email,
                    FirstName = p.UserProfile.FirstName,
                    LastName = p.UserProfile.LastName,
                    Mobile = p.UserProfile.Mobile
                }).ToList();

            return Ok(drivers);
        }

        [Route("driver/create")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateDriver([FromBody] AddDistributorDriverRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            UserAndProfileModel profile = new UserAndProfileModel()
            {
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                UserName = request.Mobile,
                Password = request.Mobile,
                Type = UserType.DRIVER,
                BusinessEntityId = entity.Id,
                AgreedTerms = true,
                UpdatedBy = userId.ToString()
            };
            var command = new CreateProfileAndUserCommand(profile);
            return ProcessCommand(command);
        }

        [Route("driver/update/{driverId}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateDriver(int driverId, [FromBody] AddDistributorDriverRequest request)
        {
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            var driver = await _queries.GetByUserId(driverId);
            if (driver == null || driver.Type != UserType.DRIVER)
            {
                return BadRequest("Invalid driver");
            }
            if (driver.BusinessEntityId != entityId)
            {
                return BadRequest("Driver is not assigned to this entity");
            }

            UserAndProfileModel profile = new UserAndProfileModel()
            {
                UserId = driverId,
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                UserName = request.Mobile,
                Password = request.Mobile,
                Type = UserType.DRIVER,
                BusinessEntityId = entityId,
                AgreedTerms = true
            };
            var command = new UpdateProfileCommand(profile, isPatch: false);
            return ProcessCommand(command);
        }

        [Route("vehicles")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DistributorVehicleModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetVehicleList([FromQuery] bool withLocation = false)
        {
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            List<DistributorVehicleModel> response = new List<DistributorVehicleModel>();
            var vehicles = await _vehQueries.GetAllList(null, null, entityId, false);

            List<VehicleLocationsViewModel> locations = new List<VehicleLocationsViewModel>();
            if (withLocation)
            {
                locations = await _locationService.GetVehicleLocations(entity.TenantId);
            }

            foreach (var veh in vehicles)
            {
                DistributorVehicleModel vehModel = DistributorVehicleModel.FromVehicleModel(veh);

                if (withLocation)
                {
                    if (locations != null)
                    {
                        var vehLocation = locations.Where(p => p.VehicleId == veh.Id).FirstOrDefault();
                        if (vehLocation != null)
                        {
                            vehModel.LastLocationLat = vehLocation.Lat;
                            vehModel.LastLocationLng = vehLocation.Lng;
                            vehModel.LastLocationUpdatedAt = vehLocation.CreatedAt;
                        }
                    }
                }

                response.Add(vehModel);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("vehicle/create")]
        [ProducesResponseType(typeof(DistributorCreateVehicleVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCreateVehicleModel()
        {
            int entityId = _identityService.GetBusinessEntityId();
            DistributorCreateVehicleVM createVehicleVM = new DistributorCreateVehicleVM()
            {
                DriverSelectList = await _queries.GetDriverSelectList(null, null, entityId),
            };
            return Ok(createVehicleVM);
        }

        [Route("vehicle/create")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateVehicle([FromBody] AddDistributorVehicleRequest request)
        {
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            AddVehicleRequest vehicle = new AddVehicleRequest()
            {
                BranchId = entity.BranchId,
                TenantId = entity.TenantId,
                BusinessEntityId = entity.Id,
                DestinationLat = request.DestinationLat,
                DestinationLng = request.DestinationLng,
                DriverId = request.DriverId,
                IsActive = request.IsActive,
                OriginLat = request.OriginLat,
                originLng = request.OriginLng,
                RegNo = request.RegNo,
                State = VehicleState.OutFromWork,
                MaxCylinders = 20
            };
            var command = new CreateVehicleCommand(vehicle);
            return ProcessCommand(command);
        }

        [HttpGet]
        [Route("vehicle/update/{vehicleId}")]
        [ProducesResponseType(typeof(DistributorUpdateVehicleVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetEditVehicleModel(int vehicleId)
        {
            int entityId = _identityService.GetBusinessEntityId();
            VehicleModel vehModel = await _vehQueries.GetDetails(vehicleId);
            if (vehModel != null && vehModel.BusinessEntityId == entityId)
            {
                DistributorUpdateVehicleVM createVehicleVM = new DistributorUpdateVehicleVM()
                {
                    Vehicle = new AddDistributorVehicleRequest
                    {
                        DestinationLat = vehModel.DestinationLat,
                        DestinationLng = vehModel.DestinationLng,
                        DriverId = vehModel.DriverId,
                        //Id = vehModel.Id,
                        IsActive = vehModel.IsActive,
                        OriginLat = vehModel.OriginLat,
                        OriginLng = vehModel.OriginLng,
                        RegNo = vehModel.RegNo
                    },
                    DriverSelectList = await _queries.GetDriverSelectList(null, null, entityId),
                };
                return Ok(createVehicleVM);
            }
            return NotFound();
        }

        [Route("vehicle/update/{vehicleId}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> EditVehicle(int vehicleId, [FromBody] AddDistributorVehicleRequest request)
        {
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            VehicleModel vehModel = await _vehQueries.GetDetails(vehicleId);
            if (vehModel == null)
            {
                return BadRequest("Invalid vehicle");
            }
            if (vehModel.BusinessEntityId != entityId)
            {
                return BadRequest("Vehicle is not assigned to this entity");
            }

            AddVehicleRequest vehicle = new AddVehicleRequest()
            {
                Id = vehicleId,
                BranchId = entity.BranchId,
                TenantId = entity.TenantId,
                BusinessEntityId = entityId,
                DestinationLat = request.DestinationLat,
                DestinationLng = request.DestinationLng,
                DriverId = request.DriverId,
                IsActive = request.IsActive,
                OriginLat = request.OriginLat,
                originLng = request.OriginLng,
                RegNo = request.RegNo,
                State = VehicleState.OutFromWork,
            };
            var command = new UpdateVehicleCommand(vehicle, false);
            return ProcessCommand(command);
        }

        [Route("customers")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DistributorCustomerModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerList()
        {
            int userId = _identityService.GetUserIdentity();
            int entityId = _identityService.GetBusinessEntityId();
            var list = await _queries.GetCustomersListOfBusinessEntity(entityId, userId);
            return Ok(list);
        }

        [HttpGet]
        [Route("customers/search/{term}")]
        [ProducesResponseType(typeof(IEnumerable<UserDetailsModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomersByTerm(string term, [FromQuery] int from = 0, [FromQuery] int size = 20, int? tenantId = null, int? branchId = null)
        {
            int entityId = _identityService.GetBusinessEntityId();
            var customers = await _queries.GetCustomersBySearch(term, from, size, tenantId, branchId, entityId);
            return Ok(customers);
        }

        [Route("customer/address/{customerId}")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCustomerAddress(int customerId, [FromBody] AddressModel request)
        {
            int userId = _identityService.GetUserIdentity();
            return await _profileMgr.CreateCustomerAddress(userId, customerId, request);
        }

        [Route("createcustomerorder/details/{branchId}")]
        [HttpGet()]
        [ProducesResponseType(typeof(PWCreateOrderVM), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCreateOrderPageDetails(int branchId)
        {
            PWCreateOrderVM response = new PWCreateOrderVM();
            response.DeliverySlotList = await _queries.GetDeliverySlotList(branchId);

            return Ok(response);
        }

        [Route("dashboard")]
        [HttpGet]
        [ProducesResponseType(typeof(DistributorDashboardModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDashboard()
        {
            int userId = _identityService.GetUserIdentity();
            int distributorId = _identityService.GetBusinessEntityId();
            var response = await _businessEntityQueries.GetBusinessEntityDashboard(distributorId, userId);
            return Ok(response);
        }

        [Authorize(Roles = "DISTRIBUTOR_ADMIN, DEALER_ADMIN")]
        [Route("checkout/businessentity/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(BusinessEntityModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBusinessEntityDetailsForCheckout(int id)
        {
            var businessEntityId = _identityService.GetBusinessEntityId();
            var businessEntity = await _businessEntityQueries.GetDetailsById(id);
            if (businessEntity != null)
            {
                if (businessEntityId != id && businessEntity.ParentBusinessEntityId != id)
                {
                    return NotFound();
                }
                //profile.PhotoUrl = $"{_avatarsUrl}user_{profile.UserId}.jpg";
                return Ok(businessEntity);
            }

            return NotFound();
        }

        [Authorize(Roles = "DISTRIBUTOR_ADMIN, DEALER_ADMIN")]
        [Route("checkout/customer/{mobile}")]
        [HttpGet]
        [ProducesResponseType(typeof(UserAndProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerDetailsForCheckout(string mobile)
        {
            int distributorId = _identityService.GetBusinessEntityId();
            var customer = await _queries.GetUserAndProfileByMobile(mobile, UserType.CUSTOMER, 1); //remove tenant hardcode
            if (customer.UserId > 0)
            {
                //profile.PhotoUrl = $"{_avatarsUrl}user_{profile.UserId}.jpg";
                return Ok(customer);
            }

            return NotFound();
        }

        #region Distributor

        [Authorize(Roles = "DISTRIBUTOR_ADMIN")]
        [Route("distributor/dealers")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DealerModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDealerList()
        {
            List<DealerModel> dealerList = new List<DealerModel>();
            int userId = _identityService.GetUserIdentity();
            int distributorId = _identityService.GetBusinessEntityId();
            List<BusinessEntityModel> dealers = await _businessEntityQueries.GetAllList(BusinessEntityType.Dealer, null, null, null, distributorId, null, null, null);

            var dealerAssetCountList = await _catalogService.GetDealerAssetCountListForDistributor();
            foreach (var dealer in dealers)
            {
                DealerModel dealerModel = new DealerModel() { Profile = dealer };
                var dealerAssetCount = dealerAssetCountList.Where(p => p.BusinessEntityId == dealer.Id).FirstOrDefault();
                if (dealerAssetCount != null)
                {
                    dealerModel.AssetCounts = dealerAssetCount.AssetCounts;
                }

                dealerList.Add(dealerModel);
            }

            return Ok(dealerList);
        }

        [Authorize(Roles = "DISTRIBUTOR_ADMIN")]
        [Route("distributor/dealer/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(DealerDetailModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDealerDetails(int id)
        {
            DealerDetailModel dealerDetail = new DealerDetailModel();
            int userId = _identityService.GetUserIdentity();
            int distributorId = _identityService.GetBusinessEntityId();
            dealerDetail.Profile = await _businessEntityQueries.GetDetailsById(id);
            if (dealerDetail.Profile == null || dealerDetail.Profile.ParentBusinessEntityId != distributorId)
            {
                return NotFound();
            }

            dealerDetail.AssetCounts = await _catalogService.GetDealerAssetCount(id);
            return Ok(dealerDetail);
        }

        #endregion

        #region Staff

        [Route("staff/create")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateStaff([FromBody] AddStaffRequest request)
        {
            List<UserType> staffUserTypes = new List<UserType>()
            {
                UserType.DRIVER, UserType.SECURITY, UserType.MARSHAL, UserType.PICKUP_DRIVER
            };

            if (!staffUserTypes.Contains(request.UserType))
            {
                return BadRequest("Not allowed");
            }

            int userId = _identityService.GetUserIdentity();
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            UserAndProfileModel profile = new UserAndProfileModel()
            {
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                UserName = request.Mobile,
                Password = request.Mobile,
                Type = request.UserType,
                BusinessEntityId = entity.Id,
                AgreedTerms = true,
                UpdatedBy = userId.ToString()
            };
            var command = new CreateProfileAndUserCommand(profile);
            var commandResult = ProcessCommand(command);
            if (commandResult is CommandResult)
            {
                var result = commandResult as CommandResult;
                if (result.IsOk && result.Content is CreateProfileResponse)
                {
                    CreateProfileResponse profResp = result.Content as CreateProfileResponse;
                    await _walletMgr.CreateWallet(profResp.UserId);
                }
            }

            return commandResult;
        }

        [Route("staff/update/{staffId}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateStaff(int staffId, [FromBody] AddStaffRequest request)
        {
            List<UserType> staffUserTypes = new List<UserType>()
            {
                UserType.DRIVER, UserType.SECURITY, UserType.MARSHAL, UserType.PICKUP_DRIVER
            };

            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            var staff = await _queries.GetByUserId(staffId);
            if (staff == null)
            {
                return BadRequest("Invalid staff");
            }

            if (!staffUserTypes.Contains(staff.Type))
            {
                return BadRequest("Invalid staff");
            }

            if (staff.BusinessEntityId != entityId)
            {
                return BadRequest("Staff is not assigned to this entity");
            }

            UserAndProfileModel profile = new UserAndProfileModel()
            {
                UserId = staffId,
                TenantId = entity.TenantId,
                BranchId = entity.BranchId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                UserName = request.Mobile,
                Password = request.Mobile,
                Type = staff.Type,
                BusinessEntityId = entityId,
                AgreedTerms = true
            };
            var command = new UpdateProfileCommand(profile, isPatch: false);
            return ProcessCommand(command);
        }

        [Route("staffs")]
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetStaffList()
        {
            int entityId = _identityService.GetBusinessEntityId();
            var staffs = await _queries.GetStaffList(null, null, entityId);
            return Ok(staffs);
        }

        [Route("staff/{staffId}")]
        [HttpGet]
        [ProducesResponseType(typeof(List<StaffModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetStaff(int staffId)
        {
            List<UserType> staffUserTypes = new List<UserType>()
            {
                UserType.DRIVER, UserType.SECURITY, UserType.MARSHAL, UserType.PICKUP_DRIVER
            };

            var user = await _queries.GetByUserId(staffId);
            if (user == null)
            {
                return BadRequest("Invalid staff");
            }

            if (!staffUserTypes.Contains(user.Type))
            {
                return BadRequest("Invalid staff");
            }

            StaffModel model = StaffModel.FromUserAndProfile(user);
            return Ok(model);
        }

        #endregion

        #region Device Dispenser/Nozzle

        [Route("dispenser/create")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateDispenser([FromBody] CreateDispenserRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Name is empty");
            }

            if (request.Nozzles == null || request.Nozzles.Count == 0 )
            {
                return BadRequest("No nozzles added");
            }

            if (request.Nozzles.Any(p => p.Name == null || p.Name == ""))
            {
                return BadRequest("Nozzle Name is empty");
            }

            Device dispenserDevice = new Device()
            {
                Name = request.Name,
                UniqueId = request.DeviceId,
                SecretKey = request.SecretKey,
                Type = DeviceType.Dispenser,
                IsActive = request.IsActive,
                BranchId = entity.BranchId,
                TenantId = entity.TenantId,
                BusinessEntityId = entity.Id,
            };
            _db.Devices.Add(dispenserDevice);
            await _db.SaveChangesAsync();

            foreach(var nozzle in request.Nozzles)
            {
                Device nozzleDevice = new Device
                {
                    Type = DeviceType.Nozzle,
                    ParentDeviceId = dispenserDevice.Id,
                    Name = nozzle.Name,
                    IsActive = nozzle.IsActive,
                    UniqueId = nozzle.DeviceId,
                    FuelType = nozzle.FuelType,
                    BranchId = entity.BranchId,
                    TenantId = entity.TenantId,
                    BusinessEntityId = entity.Id,
                };

                _db.Devices.Add(nozzleDevice);
            }

            await _db.SaveChangesAsync();

            return Ok("Dispenser added successfully");
        }

        [Route("dispenser/update/{dispenserId}")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateDispenser(int dispenserId, [FromBody] CreateDispenserRequest request)
        {
            int userId = _identityService.GetUserIdentity();
            int entityId = _identityService.GetBusinessEntityId();
            var entity = await _businessEntityQueries.GetDetailsById(entityId);
            if (entity == null)
            {
                return BadRequest("Invalid entity");
            }

            var dispenser = await _businessEntityQueries.GetDeviceById(dispenserId);
            if (dispenser == null)
            {
                return BadRequest("Invalid dispenser");
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Name is empty");
            }

            if (request.Nozzles == null || request.Nozzles.Count == 0)
            {
                return BadRequest("No nozzles added");
            }

            if (request.Nozzles.Any(p => p.Name == null || p.Name == ""))
            {
                return BadRequest("Nozzle Name is empty");
            }

            var nozzles = await _businessEntityQueries.GetDevicesByParentId(dispenserId);
            nozzles = nozzles.Where(p => p.Type == DeviceType.Nozzle).ToList();

            foreach(var nozzle in nozzles)
            {
                _db.Entry(nozzle).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            dispenser.Name = request.Name;
            dispenser.UniqueId = request.DeviceId;
            dispenser.SecretKey = request.SecretKey;
            dispenser.IsActive = request.IsActive;
            _db.Entry(dispenser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            foreach (var nozzle in request.Nozzles)
            {
                Device nozzleDevice = new Device
                {
                    Type = DeviceType.Nozzle,
                    ParentDeviceId = dispenser.Id,
                    Name = nozzle.Name,
                    IsActive = nozzle.IsActive,
                    UniqueId = nozzle.DeviceId,
                    FuelType = nozzle.FuelType,
                    BranchId = entity.BranchId,
                    TenantId = entity.TenantId,
                    BusinessEntityId = entity.Id,
                };

                _db.Devices.Add(nozzleDevice);
            }

            await _db.SaveChangesAsync();

            return Ok("Dispenser updated successfully");
        }

        [Route("dispensers")]
        [HttpGet]
        [ProducesResponseType(typeof(List<DispenserModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDispenserList()
        {
            int entityId = _identityService.GetBusinessEntityId();
            var devices = await _businessEntityQueries.GetDevices(entityId);
            var dispenserDevices = devices.Where(p => p.Type == DeviceType.Dispenser).ToList();
            List<DispenserModel> dispensers = new List<DispenserModel>();
            foreach (var dispenserDevice in dispenserDevices)
            {
                DispenserModel dispenser = new DispenserModel
                {
                    Id = dispenserDevice.Id,
                    Name = dispenserDevice.Name,
                    IsActive = dispenserDevice.IsActive,
                    DeviceId = dispenserDevice.UniqueId,
                    SecretKey = dispenserDevice?.SecretKey,
                    Nozzles = new List<NozzleModel>()
                };

                var nozzleDevices = devices.Where(p => p.ParentDeviceId == dispenserDevice.Id).ToList();
                foreach (var nozzleDevice in nozzleDevices)
                {
                    NozzleModel nozzle = new NozzleModel
                    {
                        Id = nozzleDevice.Id,
                        Name = nozzleDevice.Name,
                        IsActive = nozzleDevice.IsActive,
                        DeviceId = nozzleDevice.UniqueId,
                        FuelType = nozzleDevice.FuelType,
                    };

                    dispenser.Nozzles.Add(nozzle);
                }

                dispensers.Add(dispenser);
            }
            
            return Ok(dispensers);
        }

        [Route("dispenser/{dispenserId}")]
        [HttpGet]
        [ProducesResponseType(typeof(DispenserModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDispenser(int dispenserId)
        {
            var dispenserDevice = await _businessEntityQueries.GetDeviceById(dispenserId);
            if (dispenserDevice == null)
            {
                return BadRequest("Invalid dispenser");
            }

            DispenserModel dispenser = new DispenserModel
            {
                Id = dispenserDevice.Id,
                Name = dispenserDevice.Name,
                IsActive = dispenserDevice.IsActive,
                DeviceId = dispenserDevice.UniqueId,
                SecretKey = dispenserDevice?.SecretKey,
                Nozzles = new List<NozzleModel>()
            };

            var nozzleDevices = await _businessEntityQueries.GetDevicesByParentId(dispenserDevice.Id);
            foreach (var nozzleDevice in nozzleDevices)
            {
                NozzleModel nozzle = new NozzleModel
                {
                    Id = nozzleDevice.Id,
                    Name = nozzleDevice.Name,
                    IsActive = nozzleDevice.IsActive,
                    DeviceId = nozzleDevice.UniqueId,
                    FuelType = nozzleDevice.FuelType,
                };

                dispenser.Nozzles.Add(nozzle);
            }

            return Ok(dispenser);
        }

        #endregion
    }
}

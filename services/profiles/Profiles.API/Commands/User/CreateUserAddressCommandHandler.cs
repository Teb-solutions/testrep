using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateUserAddressCommandHandler : ICommandHandler<CreateUserAddressCommand>
    {
        private readonly ProfilesDbContext _db;
        private GeoFenceMgr _geoFenceMgr;
        private CreateUserAddressCommand _command;
        //private ApiSettings _apiSettings;
        private readonly ILogger _logger;

        public CreateUserAddressCommandHandler(ProfilesDbContext db, GeoFenceMgr geoFenceMgr, ILogger<CreateUserAddressCommandHandler> logger)
        {
            _db = db;
            _geoFenceMgr = geoFenceMgr;
            //_apiSettings = apiSettings;
            _logger = logger;
        }

        public CommandHandlerResult Handle(CreateUserAddressCommand command)
        {
            _command = command;
            var existingUser = _db.Users.Include(p => p.Profile).Where(x => x.Id == _command.UserAddress.UserId).FirstOrDefault();
            if (existingUser == null)
            {
                return CommandHandlerResult.Error($"User not found");
            }

            //Branch branch = _geoFenceMgr.GetBranchByLocation(existingUser.TenantId, _command.UserAddress.Location, _command.UserAddress.Lat, _command.UserAddress.Lng).Result;
            Pincode pincodeModel = _geoFenceMgr.GetPincodeModel(_command.UserAddress.PinCode, _command.UserAddress.Lat, _command.UserAddress.Lng);

            if (existingUser.Type == UserType.CUSTOMER)
            {
                bool isPincodeDeliverable = _geoFenceMgr.IsPincodeDeliverable(_command.UserAddress.PinCode, _command.UserAddress.Lat, _command.UserAddress.Lng);
                //bool isDeliverable = branch != null && branch.IsActive;
                if (!isPincodeDeliverable)
                {
                    var addressNotServicable = new AddressNotInService()
                    {
                        UserId = existingUser.Id,
                        Username = existingUser.UserName,
                        UserPhone = existingUser.Profile.Mobile,
                        Name = _command.UserAddress.Name,
                        City = _command.UserAddress.City,
                        Landmark = _command.UserAddress.Landmark,
                        Lat = _command.UserAddress.Lat,
                        Lng = _command.UserAddress.Lng,
                        PhoneAlternate = _command.UserAddress.PhoneAlternate,
                        Location = _command.UserAddress.Location,
                        PinCode = _command.UserAddress.PinCode,
                        State = _command.UserAddress.State,
                    };
                    _db.AddressNotInService.Add(addressNotServicable);
                    _db.SaveChanges();
                    _logger.LogWarning("CreateUserAddress Failure | Service is unavailable in this area | user - " + _command.UserAddress.UserId + " | " + existingUser.UserName + ", addr - " + _command.UserAddress.PinCode + " | " + _command.UserAddress.Lat + "," + _command.UserAddress.Lng + " | " + _command.UserAddress.Location);
                    return CommandHandlerResult.Error($"EasyGas service is unavailable in this area. Please contact customer care if you need any help.");
                }
                else
                {
                    _command.UserAddress.TenantId = pincodeModel.TenantId;
                    _command.UserAddress.BranchId = pincodeModel.BranchId;
                    _command.UserAddress.City = pincodeModel.Branch?.Name;
                }
            }
            
            if (_command.UserAddress.Id == 0) // create new user address
            {
                _db.Addresses.Add(_command.UserAddress);
                _logger.LogInformation("CreateUserAddress Success | userId - " + _command.UserAddress.UserId + " | " + existingUser.UserName + ", addr - " + _command.UserAddress.PinCode + " | " + _command.UserAddress.Lat + "," + _command.UserAddress.Lng + " | " + _command.UserAddress.Location);
            }
            else // update user address
            {
                var existingUserAddress = _db.Addresses.SingleOrDefault(p => p.Id == _command.UserAddress.Id && p.UserId == _command.UserAddress.UserId);
                if (existingUserAddress == null)
                {
                    return CommandHandlerResult.Error("No User Address found");
                }
                if (pincodeModel != null)
                {
                    existingUserAddress.TenantId = pincodeModel.TenantId;
                    existingUserAddress.BranchId = pincodeModel.BranchId;
                    existingUserAddress.City = pincodeModel.Branch.Name;
                }
                    
                existingUserAddress.Name = _command.UserAddress.Name;
                //existingUserAddress.City = _command.UserAddress.City;
                existingUserAddress.Details = _command.UserAddress.Details;
                existingUserAddress.Landmark = _command.UserAddress.Landmark;
                existingUserAddress.State = _command.UserAddress.State;
                existingUserAddress.Location = _command.UserAddress.Location;
                existingUserAddress.Lat = _command.UserAddress.Lat;
                existingUserAddress.Lng = _command.UserAddress.Lng;
                existingUserAddress.PinCode = _command.UserAddress.PinCode;
                existingUserAddress.PhoneAlternate = _command.UserAddress.PhoneAlternate;
            }


            //_db.UserId = _command.Data.UserAndProfile.UpdatedBy;
            return CommandHandlerResult.OkDelayed(this,
                x => new CreateUserAddressResponse
                {
                    UserAddressId = _command.UserAddress.Id,
                    BranchId = (int)_command.UserAddress.BranchId
                });
        }
    }
}

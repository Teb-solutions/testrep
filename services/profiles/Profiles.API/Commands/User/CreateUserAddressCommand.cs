using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateUserAddressCommand : CommandBase
    {
        public UserAddress UserAddress;
        public CreateUserAddressCommand(UserAndAddressModel model)
        {
            UserAddress = CreateAddressFromModel(model);
        }

        private UserAddress CreateAddressFromModel(UserAndAddressModel model)
        {
            var address = new UserAddress()
            {
                Id = model.Address.UserAddressId,
                Name = model.Address.Name,
                Details = model.Address.Details,
                Lat = model.Address.Lat,
                Lng = model.Address.Lng,
                City = model.Address.City,
                Landmark = model.Address.Landmark,
                Location = model.Address.Location,
                PhoneAlternate = model.Address.PhoneAlternate,
                PinCode = model.Address.PinCode,
                State = model.Address.State,
                UserId = model.UserId
            };

            return address;
        }
        protected override IEnumerable<string> OnValidation()
        {
            if (UserAddress == null)
            {
                yield return "invalid or no payload received";
            }
            else
            {
                if (UserAddress.UserId <= 0)
                {
                    yield return "User is missing";
                }

                if (string.IsNullOrEmpty(UserAddress.Name))
                {
                    yield return "House No is missing";
                }
                if (string.IsNullOrEmpty(UserAddress.Details))
                {
                    yield return "Street No is missing";
                }
                if (string.IsNullOrEmpty(UserAddress.Location))
                {
                    yield return "Location is missing";
                }
                if (string.IsNullOrEmpty(UserAddress.PinCode))
                {
                    yield return "PinCode is missing";
                }
                if (UserAddress.Lat < -90 || UserAddress.Lat > 90 )
                {
                    yield return "Lat is invalid";
                }
                if (UserAddress.Lng < -180 || UserAddress.Lng > 180)
                {
                    yield return "Lng is invalid";
                }
            }
        }
    }
}

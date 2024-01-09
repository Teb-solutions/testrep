using EasyGas.Services.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class DeleteUserAddressCommand : CommandBase
    {
        public int UserAddressId { get; }
        public int UserId { get; }

        public DeleteUserAddressCommand(int userAddressId, int userId)
        {
            UserAddressId = userAddressId;
            UserId = userId;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (UserAddressId <= 0)
            {
                yield return "Missing or invalid User Address";
            }
        }
    }
}

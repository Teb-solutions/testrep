using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class DeleteProfileImageCommand: CommandBase
    {
        public int UserId { get; }
        public DeleteProfileImageCommand(int userid)
        {
            UserId = userid;
        }
        protected override IEnumerable<string> OnValidation()
        {
            if (UserId <= 0)
            {
                yield return "Missing or invalid userId";
            }
        }
    }
}

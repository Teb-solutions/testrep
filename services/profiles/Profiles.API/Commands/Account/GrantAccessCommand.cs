using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class GrantAccessCommand : CommandBase
    {

        public LoginModel Data { get; }
        public GrantAccessCommand(LoginModel data)
        {
            Data = data;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Data.GrantType != LoginModel.PasswordGrantType && Data.GrantType != LoginModel.OtpGrantType)
            {
                yield return $"Invalid grantType: {Data.GrantType}";
            }
            if (Data.GrantType == LoginModel.PasswordGrantType)
            {
                if (String.IsNullOrEmpty(Data.Credentials))
                {
                    yield return $"Invalid password";
                }
            }
            else if (Data.GrantType == LoginModel.OtpGrantType)
            {
                
            }
        }
    }
}

using Autofac;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class ProfilesCommandBus : CommandBus<ProfilesDbContext>
    {
        public ProfilesCommandBus(ILifetimeScope scope) : base(scope)
        {
        }
    }
}

using Autofac;
using EasyGas.BuildingBlocks.EventBus.Abstractions;
using EasyGas.BuildingBlocks.EventBus.EventBusRabbitMQ;
using MassTransit;
using Profiles.API.IntegrationEvents;
using Profiles.API.IntegrationEvents.Consumers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Profiles.API.Infrastructure.AutofacModules
{
    public class ApplicationModule
        : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProfilesIntegrationEventService>()
                .As<IProfilesIntegrationEventService>()
                .InstancePerLifetimeScope();
        }
    }
}

using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Queries;
using MassTransit;
using Microsoft.Extensions.Logging;
using Profiles.API.Services;
using Profiles.API.ViewModels.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class TestIntegrationEventConsumer : IConsumer<TestIntegrationEvent>
    {
        private readonly ILogger<TestIntegrationEventConsumer> _logger;

        public TestIntegrationEventConsumer(
            IOrderService orderService,
            IVehicleQueries vehicleQueries,
            NotificationBroadcastMgr notificationBroadcastMgr,
            ILogger<TestIntegrationEventConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<TestIntegrationEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Profiles.API TestIntegrationEvent consume started. Created Order Id : {newOrderId}", evt.OrderId);

            await Task.FromResult("test");
        }
    }
}

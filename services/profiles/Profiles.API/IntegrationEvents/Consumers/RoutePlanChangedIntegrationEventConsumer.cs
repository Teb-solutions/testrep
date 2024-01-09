using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class RoutePlanChangedIntegrationEventConsumer : IConsumer<RoutePlanChangedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly ILogger<RoutePlanChangedIntegrationEventConsumer> _logger;

        public RoutePlanChangedIntegrationEventConsumer(
            NotificationMgr notiMgr,
            ILogger<RoutePlanChangedIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<RoutePlanChangedIntegrationEvent> context)
        {
            var @event = context.Message;
            _logger.LogInformation("----- Handling Profiles.API RoutePlanChangedIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);
            
            if (@event.OrderAssignedDriverIds != null && @event.OrderAssignedDriverIds.Count > 0)
            {
                await _notiMgr.AddDriverOrderAssignedNotification(@event.OrderAssignedDriverIds);
            }

            _logger.LogInformation("Profiles.API RoutePlanChangedIntegrationEventConsumer {IntegrationEventId} at {AppName} consumed successfully", @event.Id, Program.AppName);
        }
    }
}

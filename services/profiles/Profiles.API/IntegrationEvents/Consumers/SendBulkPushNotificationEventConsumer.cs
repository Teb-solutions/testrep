using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class SendBulkPushNotificationEventConsumer : IConsumer<SendBulkPushNotificationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly ILogger<SendBulkPushNotificationEventConsumer> _logger;

        public SendBulkPushNotificationEventConsumer(
            NotificationMgr notiMgr,
            ILogger<SendBulkPushNotificationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<SendBulkPushNotificationEvent> context)
        {
            var @event = context.Message;
            _logger.LogInformation("----- Handling Profiles.API SendBulkPushNotificationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

             await _notiMgr.SendBulkNotification(@event);

            _logger.LogInformation("Profiles.API SendBulkPushNotificationEventConsumer {IntegrationEventId} at {AppName} consumed successfully", @event.Id, Program.AppName);
        }
    }
}

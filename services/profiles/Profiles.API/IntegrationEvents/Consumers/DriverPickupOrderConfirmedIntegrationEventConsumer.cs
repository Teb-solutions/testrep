using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class DriverPickupOrderConfirmedIntegrationEventConsumer : IConsumer<DriverPickupOrderConfirmedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<DriverPickupOrderConfirmedIntegrationEventConsumer> _logger;

        public DriverPickupOrderConfirmedIntegrationEventConsumer(
            NotificationMgr notiMgr,
            ISmsSender smsSender,
            ILogger<DriverPickupOrderConfirmedIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<DriverPickupOrderConfirmedIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling Profiles.API DriverPickupOrderConfirmedIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _notiMgr.AddRelaypointPickupOrderAssignedNotification(@event.RelaypointId, @event.DriverId, NotificationCategory.DRIVER_PICKUP_ORDERED, @event.Code);

                await _smsSender.SendSmsToDriverForRelaypointOrderConfirmation(@event.DriverId, @event.Code, @event.OTP);

                _logger.LogInformation("Profiles.API DriverPickupOrderConfirmedIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);

            }
        }
    }
}

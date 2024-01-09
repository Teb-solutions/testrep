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
    public class CustomerOrderDispatchedIntegrationEventConsumer : IConsumer<CustomerOrderDispatchedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<CustomerOrderDispatchedIntegrationEventConsumer> _logger;

        public CustomerOrderDispatchedIntegrationEventConsumer(
            NotificationMgr notiMgr,
            ISmsSender smsSender,
            ILogger<CustomerOrderDispatchedIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerOrderDispatchedIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling CustomerOrderDispatchedIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.OrderCode + "  is on the way.");
                await _smsSender.SendSmsToCustomerForOrderDispatch(@event.UserId, @event.OrderDeliveryOTP);

                _logger.LogInformation("Profiles.API CustomerOrderDispatchedIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);

            }
        }
    }
}

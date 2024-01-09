using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class CustomerOrderCancelledIntegrationEventConsumer : IConsumer<CustomerOrderCancelledIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly WalletMgr _walletMgr;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<CustomerOrderCancelledIntegrationEventConsumer> _logger;

        public CustomerOrderCancelledIntegrationEventConsumer(
            NotificationMgr notiMgr,
            WalletMgr walletMgr,
            ISmsSender smsSender,
            ILogger<CustomerOrderCancelledIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _walletMgr = walletMgr ?? throw new ArgumentNullException(nameof(walletMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerOrderCancelledIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling CustomerOrderCancelledIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.OrderCode + " is cancelled.");
                if (@event.DriverId != null)
                {
                    await _notiMgr.AddDriverOrderCancelledNotification((int)@event.DriverId, @event.OrderCode);
                }

                if (@event.OfferAmount != null && @event.OfferAmount > 0)
                {
                    await _walletMgr.CancelOrderTransaction(@event.OrderId, @event.UserId, @event.TenantId,
                        @event.BranchId, (int)@event.OfferCouponId, @event.OfferCouponName, (decimal)@event.OfferAmount, (CouponType)@event.OfferCouponType);
                }

                _logger.LogInformation("Profiles.API CustomerOrderCancelledIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);
            }
        }
    }
}

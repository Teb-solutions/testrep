using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class CustomerPickupOrderConfirmedIntegrationEventConsumer : IConsumer<CustomerPickupOrderConfirmedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly WalletMgr _walletMgr;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomerPickupOrderConfirmedIntegrationEventConsumer> _logger;

        public CustomerPickupOrderConfirmedIntegrationEventConsumer(
            NotificationMgr notiMgr,
            WalletMgr walletMgr,
            ISmsSender smsSender,
            IEmailSender emailSender,
            ILogger<CustomerPickupOrderConfirmedIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _walletMgr = walletMgr ?? throw new ArgumentNullException(nameof(walletMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerPickupOrderConfirmedIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling Profiles.API CustomerPickupOrderConfirmedIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.Code + " has been confirmed.");
                await _notiMgr.AddRelaypointPickupOrderAssignedNotification(@event.RelaypointId, @event.UserId, NotificationCategory.CUSTOMER_PICKUP_ORDERED, @event.Code);

                await _smsSender.SendSmsToCustomerForOrderConfirmation(@event.UserId);
                await _emailSender.SendEmailToAdminForNewOrder(@event.UserId, @event.DeliverySlotName, @event.BranchId);

                if (@event.OfferAmount != null && @event.OfferAmount > 0)
                {
                    await _walletMgr.CreateOrderTransaction(@event.OrderId, @event.UserId, @event.TenantId,
                        @event.BranchId, @event.UserMobile, (int)@event.OfferCouponId, @event.OfferCouponName, (decimal)@event.OfferAmount, (CouponType)@event.OfferCouponType);
                }

                _logger.LogInformation("Profiles.API CustomerPickupOrderConfirmedIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);

            }
        }
    }
}

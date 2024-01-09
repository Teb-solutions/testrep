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
    public class CustomerExpressOrderConfirmedIntegrationEventConsumer : IConsumer<CustomerExpressOrderConfirmedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly NotificationBroadcastMgr _notificationBroadcastMgr;
        private readonly WalletMgr _walletMgr;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomerExpressOrderConfirmedIntegrationEventConsumer> _logger;

        public CustomerExpressOrderConfirmedIntegrationEventConsumer(
            ISmsSender smsSender,
            IEmailSender emailSender,
            NotificationMgr notiMgr,
            NotificationBroadcastMgr notificationBroadcastMgr,
            WalletMgr walletMgr,
            ILogger<CustomerExpressOrderConfirmedIntegrationEventConsumer> logger)
        {
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _walletMgr = walletMgr ?? throw new ArgumentNullException(nameof(walletMgr));
            _notificationBroadcastMgr = notificationBroadcastMgr ?? throw new ArgumentNullException(nameof(notificationBroadcastMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerExpressOrderConfirmedIntegrationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation("Profiles.API CustomerExpressOrderConfirmedIntegrationEventConsumer started. Confirmed Order Id : {newOrderId}", @event.OrderId);

            if (@event.BroadcastStopDriverList.Count > 0)
            {
                await _notificationBroadcastMgr.BroadcastStopToDriverWhenOrderAccepted(@event.OrderId, @event.Code, @event.BroadcastStopDriverList);
            }

            await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.Code + " has been confirmed.");

            await _notiMgr.AddDriverOrderAssignedNotification(new List<int> { @event.DriverId });

            await _smsSender.SendSmsToCustomerForOrderConfirmation(@event.UserId);
            await _emailSender.SendEmailToAdminForNewOrder(@event.UserId, @event.DeliverySlotName, @event.BranchId);

            //dispatched noti
            await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.Code + " is on the way.");
            await _smsSender.SendSmsToCustomerForOrderDispatch(@event.OrderId, @event.DeliveryOtp);

            if (@event.OfferAmount != null && @event.OfferAmount > 0)
            {
                await _walletMgr.CreateOrderTransaction(@event.OrderId, @event.UserId, @event.TenantId,
                    @event.BranchId, @event.UserMobile, (int)@event.OfferCouponId, @event.OfferCouponName, (decimal)@event.OfferAmount, (CouponType)@event.OfferCouponType);
            }

            _logger.LogInformation("CustomerExpressOrderConfirmedIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);
        }
    }
}

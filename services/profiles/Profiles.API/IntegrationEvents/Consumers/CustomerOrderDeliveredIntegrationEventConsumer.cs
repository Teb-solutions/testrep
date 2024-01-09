using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class CustomerOrderDeliveredIntegrationEventConsumer : IConsumer<CustomerOrderDeliveredIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly InvoiceMgr _invoiceMgr;
        private readonly WalletMgr _walletMgr;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomerOrderDeliveredIntegrationEventConsumer> _logger;

        public CustomerOrderDeliveredIntegrationEventConsumer(
            NotificationMgr notiMgr,
            InvoiceMgr invoiceMgr,
            WalletMgr walletMgr,
            ISmsSender smsSender,
            IEmailSender emailSender,
            ILogger<CustomerOrderDeliveredIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _walletMgr = walletMgr ?? throw new ArgumentNullException(nameof(walletMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _invoiceMgr = invoiceMgr ?? throw new ArgumentNullException(nameof(invoiceMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerOrderDeliveredIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling Profiles.API CustomerOrderDeliveredIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                var invoiceResponse = await _invoiceMgr.AddInvoiceAndDigitalVoucher(@event);
                if (!invoiceResponse.IsSuccess)
                {
                    _logger.LogCritical("Profiles.API CustomerOrderDeliveredIntegrationEventConsumer invoice generation failure for OrderId : {newOrderId} | {message} ", @event.OrderId, invoiceResponse.Message);
                }

                await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.Code + " was delivered.");
                await _smsSender.SendSmsToCustomerForOrderDelivered(@event.UserId, @event.Code, @event.Code, invoiceResponse.InvoicePdfUrl);
                if (@event.DeliveredByAdmin && @event.DriverId != null && !@event.IsPickupOrder)
                {
                    await _notiMgr.AddDriverOrderDeliveredNotification((int)@event.DriverId, @event.Code);
                }

                if (@event.ReferredUserId != null || @event.TotalDiscountPrice > 0)
                {
                    var walletTrans = await _walletMgr.DeliverOrderTransaction(@event.OrderId, @event.UserId, @event.TenantId,
                        @event.ReferredUserId, @event.BusinessEntityId, @event.OfferCouponName, @event.TotalDiscountPrice);
                }

                _logger.LogInformation("Profiles.API CustomerOrderDeliveredIntegrationEventConsumer consumed successfully. Delivered Order Id : {newOrderId}", @event.OrderId);
            }
        }
    }
}

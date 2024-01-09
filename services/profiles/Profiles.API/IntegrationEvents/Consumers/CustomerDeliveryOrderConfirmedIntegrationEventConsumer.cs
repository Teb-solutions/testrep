using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class CustomerDeliveryOrderConfirmedIntegrationEventConsumer : IConsumer<CustomerDeliveryOrderConfirmedIntegrationEvent>
    {
        private readonly NotificationMgr _notiMgr;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly IVehicleQueries _queries;
        private readonly WalletMgr _walletMgr;
        private readonly IProfilesIntegrationEventService _profilesIntegrationEventService;
        private readonly ILogger<CustomerDeliveryOrderConfirmedIntegrationEventConsumer> _logger;

        public CustomerDeliveryOrderConfirmedIntegrationEventConsumer(
            NotificationMgr notiMgr,
            ISmsSender smsSender,
            IEmailSender emailSender,
            IVehicleQueries queries,
            WalletMgr walletMgr,
            IProfilesIntegrationEventService profilesIntegrationEventService,
            ILogger<CustomerDeliveryOrderConfirmedIntegrationEventConsumer> logger)
        {
            _notiMgr = notiMgr ?? throw new ArgumentNullException(nameof(notiMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _walletMgr = walletMgr ?? throw new ArgumentNullException(nameof(walletMgr));
            _profilesIntegrationEventService = profilesIntegrationEventService;
            _queries = queries;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerDeliveryOrderConfirmedIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling CustomerDeliveryOrderConfirmedIntegrationEventConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _notiMgr.AddNotification(@event.UserId, NotificationType.INFO, NotificationCategory.CUSTOMER_ORDER_STATUS_CHANGE, "Your order #" + @event.Code + " has been confirmed.");

                await _smsSender.SendSmsToCustomerForOrderConfirmation(@event.UserId);
                await _emailSender.SendEmailToAdminForNewOrder(@event.UserId, @event.DeliverySlotName, @event.BranchId);

                if (@event.OfferAmount != null && @event.OfferAmount > 0)
                {
                    await _walletMgr.CreateOrderTransaction(@event.OrderId, @event.UserId, @event.TenantId,
                        @event.BranchId, @event.UserMobile, (int)@event.OfferCouponId, @event.OfferCouponName, (decimal)@event.OfferAmount, (CouponType)@event.OfferCouponType);
                }

                VehiclePlanningStartedIntegrationEvent @planningEvent = new VehiclePlanningStartedIntegrationEvent()
                {
                    BranchId = @event.BranchId,
                    TenantId = @event.TenantId,
                    Vehicles = await _queries.GetAllListForPlanning(@event.TenantId, @event.BranchId, null, true)
                };
                await _profilesIntegrationEventService.PublishEventThroughEventBusAsync(@planningEvent);

                _logger.LogInformation("Profiles.API CustomerDeliveryOrderConfirmedIntegrationEventConsumer consumed successfully. Confirmed Order Id : {newOrderId}", @event.OrderId);
            }
        }
    }
}

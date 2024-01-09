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
    public class CustomerExpressOrderCreatedIntegrationEventConsumer : IConsumer<CustomerExpressOrderCreatedIntegrationEvent>
    {
        private readonly IOrderService _orderService;
        private readonly IVehicleQueries _vehicleQueries;
        private readonly NotificationBroadcastMgr _notificationBroadcastMgr;
        private readonly ILogger<CustomerExpressOrderCreatedIntegrationEventConsumer> _logger;

        public CustomerExpressOrderCreatedIntegrationEventConsumer(
            IOrderService orderService,
            IVehicleQueries vehicleQueries,
            NotificationBroadcastMgr notificationBroadcastMgr,
            ILogger<CustomerExpressOrderCreatedIntegrationEventConsumer> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _vehicleQueries = vehicleQueries ?? throw new ArgumentNullException(nameof(vehicleQueries));
            _notificationBroadcastMgr = notificationBroadcastMgr ?? throw new ArgumentNullException(nameof(notificationBroadcastMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CustomerExpressOrderCreatedIntegrationEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("ExpressOrderCreatedIntegrationEvent consumed started. Created Order Id : {newOrderId}", evt.OrderId);

            UpdateExpressBroadcastRequest req = new UpdateExpressBroadcastRequest()
            {
                OrderId = evt.OrderId,
                Vehicles = await _vehicleQueries.GetAllList(evt.TenantId, evt.BranchId, null)
            };

            var orderServiceResponse = await _orderService.UpdateExpressOrderBroadcastDrivers(req);

            if (orderServiceResponse != null && orderServiceResponse.Vehicles != null && orderServiceResponse.Vehicles.Count > 0)
            {
                await _notificationBroadcastMgr.BroadcastToDrivers(orderServiceResponse, evt.ItemsSummary);
            }

            //throw new Exception();
            _logger.LogInformation("ExpressOrderCreatedIntegrationEvent consumed successfully. Created Order Id : {newOrderId}", evt.OrderId);
            //await Task.FromResult("test");
        }
    }
}

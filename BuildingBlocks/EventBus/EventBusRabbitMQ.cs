using EasyGas.BuildingBlocks.EventBus.Abstractions;
using EasyGas.BuildingBlocks.EventBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EasyGas.BuildingBlocks.EventBus.EventBusRabbitMQ
{
    public class EventBusRabbitMQ : IEventBus
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger _logger;

        public EventBusRabbitMQ(IPublishEndpoint publishEndpoint, ILogger<EventBusRabbitMQ> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger;
        }

        public async Task Publish<T>(T @event)
        {
            _logger.LogInformation("Publishing event " + nameof(@event));
            await _publishEndpoint.Publish(@event);
        }
    }
}

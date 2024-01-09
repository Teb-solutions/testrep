using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Services;
using EasyGas.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents.Consumers
{
    public class AverageRatingChangedIntegrationEventConsumer : IConsumer<AverageRatingChangedIntegrationEvent>
    {
        private readonly ProfileMgr _profileMgr;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<AverageRatingChangedIntegrationEventConsumer> _logger;

        public AverageRatingChangedIntegrationEventConsumer(
            ProfileMgr profileMgr,
            ISmsSender smsSender,
            ILogger<AverageRatingChangedIntegrationEventConsumer> logger)
        {
            _profileMgr = profileMgr ?? throw new ArgumentNullException(nameof(profileMgr));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<AverageRatingChangedIntegrationEvent> context)
        {
            var @event = context.Message;
            //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling Profiles.API AverageRatingChangedIntegrationEventConsumerConsumer integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                if (@event.IsCustomer)
                {
                    await _profileMgr.UpdateAverageRating(@event.UserId, @event.AverageRating, UserType.CUSTOMER);
                }
                else if (@event.IsDriver)
                {
                    await _profileMgr.UpdateAverageRating(@event.UserId, @event.AverageRating, UserType.DRIVER);
                }
                else if (@event.IsRelaypoint)
                {
                    await _profileMgr.UpdateAverageRating(@event.UserId, @event.AverageRating, UserType.RELAY_POINT);
                }

                _logger.LogInformation("Profiles.API AverageRatingChangedIntegrationEventConsumerConsumer consumed successfully for {eventId}", @event.Id);

            }
        }
    }
}

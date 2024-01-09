using EasyGas.BuildingBlocks.EventBus.Abstractions;
using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.BuildingBlocks.IntegrationEventLogEF;
using EasyGas.BuildingBlocks.IntegrationEventLogEF.Services;
using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents
{
    public class ProfilesIntegrationEventService : IProfilesIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly ProfilesDbContext _profilesContext;
        private readonly IIntegrationEventLogService _eventLogService;
        private readonly ILogger<ProfilesIntegrationEventService> _logger;

        public ProfilesIntegrationEventService(IEventBus eventBus,
            ProfilesDbContext profilesContext,
            Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory,
            ILogger<ProfilesIntegrationEventService> logger)
        {
            _profilesContext = profilesContext ?? throw new ArgumentNullException(nameof(profilesContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_profilesContext.Database.GetDbConnection());
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
        {
            var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

            foreach (var logEvt in pendingLogEvents)
            {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", logEvt.EventId, "EasyGas", logEvt.IntegrationEvent);

                try
                {
                    await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                    await _eventBus.Publish(logEvt.IntegrationEvent);
                    await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}", logEvt.EventId, "EasyGas");

                    await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
                }
            }
        }

        public async Task AddAndSaveEventAsync(IntegrationEvent evt)
        {
            _logger.LogInformation("----- Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

            await _eventLogService.SaveEventAsync(evt, _profilesContext.GetCurrentTransaction());
        }

        public async Task PublishEventThroughEventBusAsync(IntegrationEvent evt)
        {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, "EasyGas", evt);

                try
                {
                   // await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                    await _eventBus.Publish(evt);
                    //await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}", evt.Id, "EasyGas");

                    //await _eventLogService.MarkEventAsFailedAsync(evt.Id);
                }
        }
    }
}

using EasyGas.BuildingBlocks.EventBus.Events;
using System;
using System.Threading.Tasks;

namespace Profiles.API.IntegrationEvents
{
    public interface IProfilesIntegrationEventService
    {
        Task PublishEventsThroughEventBusAsync(Guid transactionId);
        Task AddAndSaveEventAsync(IntegrationEvent evt);
        Task PublishEventThroughEventBusAsync(IntegrationEvent evt);
    }
}


using EasyGas.BuildingBlocks.EventBus.Events;
using System.Threading.Tasks;

namespace EasyGas.BuildingBlocks.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task Publish<T>(T @event);
    }
}

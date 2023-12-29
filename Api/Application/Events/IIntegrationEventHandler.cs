using System.Threading.Tasks;

namespace Api.Application.Events
{
    public interface IIntegrationEventHandler<in TEvent>
        where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent @event);
    }
}

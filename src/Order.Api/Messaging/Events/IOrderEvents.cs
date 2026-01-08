using Order.Api.Models;

namespace Order.Api.Messaging.Events
{
    public interface IOrderEvents
    {
        Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default);
    }
}

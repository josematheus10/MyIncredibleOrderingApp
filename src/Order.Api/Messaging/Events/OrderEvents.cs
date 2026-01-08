using Order.Api.Data.Entities;
using Order.Api.Data.Repository;
using Order.Api.Models;
using System.Text.Json;

namespace Order.Api.Messaging.Events
{
    public class OrderEvents : IOrderEvents
    {
        private readonly IOutboxMessageRepository _outboxMessageRepository;

        public OrderEvents(IOutboxMessageRepository outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository ?? throw new ArgumentNullException(nameof(outboxMessageRepository));
        }

        public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default)
        {
            var outboxMessage = new OutboxMessageEntity
            {
                Id = Guid.NewGuid(),
                Type = "order.created",
                Payload = JsonSerializer.Serialize(orderCreatedEvent),
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
                RetryCount = 0,
                Error = null
            };

            await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);
        }
    }
}

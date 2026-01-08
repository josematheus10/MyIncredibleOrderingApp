using Order.Api.Data.Entities;

namespace Order.Api.Data.Repository
{
    public interface IOutboxMessageRepository
    {
        Task AddAsync(OutboxMessageEntity outboxMessage, CancellationToken cancellationToken = default);
        Task<IEnumerable<OutboxMessageEntity>> GetUnprocessedMessagesAsync(int batchSize = 10, CancellationToken cancellationToken = default);
        Task<OutboxMessageEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Update(OutboxMessageEntity outboxMessage);
    }
}

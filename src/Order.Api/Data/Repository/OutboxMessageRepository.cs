using Microsoft.EntityFrameworkCore;
using Order.Api.Data.Entities;

namespace Order.Api.Data.Repository
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly AppDbContext _context;

        public OutboxMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessageEntity outboxMessage, CancellationToken cancellationToken = default)
        {
            await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        public async Task<IEnumerable<OutboxMessageEntity>> GetUnprocessedMessagesAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<OutboxMessageEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public void Update(OutboxMessageEntity outboxMessage)
        {
            _context.OutboxMessages.Update(outboxMessage);
        }
    }
}

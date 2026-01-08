using Order.Api.Data.Repository;
using Order.Api.Data.UnitOfWork;
using Order.Api.Messaging;
using System.Text.Json;

namespace Order.Api.Outbox
{
    public class OutboxProcessor
    {
        private const int machBatchSize = 20;
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMqPublisher<object> _mqPublisher;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(
            IOutboxMessageRepository outboxMessageRepository,
            IUnitOfWork unitOfWork,
            IMqPublisher<object> mqPublisher,
            ILogger<OutboxProcessor> logger)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _unitOfWork = unitOfWork;
            _mqPublisher = mqPublisher;
            _logger = logger;
        }

        public async Task<int> Execute(CancellationToken cancellationToken = default)
        {
            // 1 - pegue as mensagens não processadas da tabela OutboxMessages
            var unprocessedMessages = await _outboxMessageRepository
                .GetUnprocessedMessagesAsync(machBatchSize, cancellationToken);

            var messagesList = unprocessedMessages.ToList();
            if (!messagesList.Any())
            {
                _logger.LogInformation("No unprocessed messages found in outbox");
                return 0;
            }

            _logger.LogInformation("Processing {Count} outbox messages", messagesList.Count);

            int processedCount = 0;

            foreach (var message in messagesList)
            {
                try
                {
                    // 2 - publique-as na fila RabbitMQ e aguarde a confirmação (ACK)
                    var payload = JsonSerializer.Deserialize<object>(message.Payload);
                    await _mqPublisher.PublishAsync(payload!, message.Type, cancellationToken);

                    // 3 - somente após ACK confirmado, marque como processada
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    
                    message.ProcessedAt = DateTime.UtcNow;
                    _outboxMessageRepository.Update(message);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    processedCount++;
                    _logger.LogInformation("Outbox message processed and acknowledged successfully: Id={MessageId}, Type={MessageType}", 
                        message.Id, message.Type);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to process outbox message (ACK not received): Id={MessageId}, Type={MessageType}, RetryCount={RetryCount}", 
                        message.Id, message.Type, message.RetryCount);
                }
            }

            _logger.LogInformation("Outbox processing completed. Processed: {ProcessedCount}/{TotalCount}", 
                processedCount, messagesList.Count);

            return processedCount;
        }
    }
}

using Order.Api.Data.Repository;
using Order.Api.Data.UnitOfWork;
using Order.Api.Messaging;
using System.Text.Json;

namespace Order.Api.Outbox
{
    public class OutboxProcessor
    {
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

        public async Task Execute(int machBatchSize, CancellationToken cancellationToken = default)
        {
            // 1 - pegue as mensagens não processadas da tabela OutboxMessages e carrega na memória
            var unprocessedMessages = await _outboxMessageRepository
                .GetUnprocessedMessagesAsync(machBatchSize, cancellationToken);

            var messagesList = unprocessedMessages.ToList();
            if (!messagesList.Any())
            {
                _logger.LogInformation("No unprocessed messages found in outbox");
                return;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // 2 - publique todas as mensagens na fila RabbitMQ em lote
                foreach (var message in messagesList)
                {
                    var payload = JsonSerializer.Deserialize<object>(message.Payload);
                    await _mqPublisher.PublishAsync(payload!, message.Type, cancellationToken);

                    // 3 - somente após todas as publicações bem-sucedidas, marque todas como processadas

                    message.ProcessedAt = DateTime.UtcNow; 

                    _outboxMessageRepository.Update(message);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}

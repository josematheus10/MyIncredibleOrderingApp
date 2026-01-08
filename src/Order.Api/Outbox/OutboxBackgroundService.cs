namespace Order.Api.Outbox
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

        public OutboxBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OutboxBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Outbox Background Service is stopping");
        }

        private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var processedCount = await outboxProcessor.Execute(cancellationToken);

                if (processedCount > 0)
                {
                    _logger.LogInformation("Outbox Background Service processed {Count} messages", processedCount);
                }
            }
          
        }
    }
}

using Polly;
using Polly.CircuitBreaker;
using System.Threading;

namespace Order.Api.Outbox
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

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
                using var scope = _serviceProvider.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                await outboxProcessor.Execute(2000,stoppingToken);

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Outbox Background Service is stopping");
        }
       
    }
}

using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Order.Api.Messaging
{
    public class MqPublisher<T> : IMqPublisher<T>, IDisposable where T : class
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<MqPublisher<T>> _logger;
        private readonly string _exchangeName;

        public MqPublisher(IConfiguration configuration, ILogger<MqPublisher<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                Port = int.Parse(configuration["RabbitMQ:Port"]),
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"],
                VirtualHost = configuration["RabbitMQ:VirtualHost"]
            };

            _exchangeName = configuration["RabbitMQ:ExchangeName"];

            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

                _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                ).GetAwaiter().GetResult();

                _logger.LogInformation("RabbitMQ connection established successfully for {MessageType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection for {MessageType}", typeof(T).Name);
                throw;
            }
        }

        public async Task PublishAsync(T message, string routingKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Message published successfully: Type={MessageType}, RoutingKey={RoutingKey}", 
                    typeof(T).Name, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message: Type={MessageType}, RoutingKey={RoutingKey}", 
                    typeof(T).Name, routingKey);
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _channel?.Dispose();
                _connection?.CloseAsync().GetAwaiter().GetResult();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection closed successfully for {MessageType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ resources for {MessageType}", typeof(T).Name);
            }
        }
    }
}

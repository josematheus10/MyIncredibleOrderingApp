namespace Order.Api.Messaging
{
    public interface IMqPublisher<in T> where T : class
    {
        Task PublishAsync(T message, string routingKey, CancellationToken cancellationToken = default);
    }
}

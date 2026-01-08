namespace Order.Api.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(Guid customerId, decimal totalValue, CancellationToken cancellationToken = default);
    }
}

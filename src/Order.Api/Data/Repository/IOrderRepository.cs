using Order.Api.Data.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Api.Data.Repository
{
    public interface IOrderRepository
    {
        Task<Guid> CreateOrderAsync(Guid id, Guid customerId, decimal totalValue, CancellationToken cancellationToken = default);
       
    }
}

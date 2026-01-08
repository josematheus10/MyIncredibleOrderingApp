using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Order.Api.Data.Entities;

namespace Order.Api.Data.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateOrderAsync(Guid id, Guid customerId, decimal totalValue, CancellationToken cancellationToken = default)
        {
            var order = new OrderEntity
            {
                Id = id,
                CustomerId = customerId,
                TotalValue = totalValue,
                Status = "PROCESSANDO",
                CreationDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order, cancellationToken);
            
            return order.Id;
        }
    }
}

using Order.Api.Data.Repository;
using Order.Api.Data.UnitOfWork;
using Order.Api.Messaging;
using Order.Api.Messaging.Events;
using Order.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderEvents _orderCreatedPublisher;

        public OrderService(
            IOrderRepository orderRepository, 
            IUnitOfWork unitOfWork,
            IOrderEvents orderCreatedPublisher)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _orderCreatedPublisher = orderCreatedPublisher ?? throw new ArgumentNullException(nameof(orderCreatedPublisher));
        }

        public async Task<Guid> CreateOrderAsync(Guid customerId, decimal totalValue, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var orderId = Guid.NewGuid();

                await _orderRepository.CreateOrderAsync(orderId, customerId, totalValue, cancellationToken);

                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = orderId,
                    CustomerId = customerId,
                    TotalValue = totalValue,
                    Status = "CRIANDO",
                    CreationDate = DateTime.UtcNow
                };

                await _orderCreatedPublisher.PublishOrderCreatedAsync(orderCreatedEvent, cancellationToken);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return orderId;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}

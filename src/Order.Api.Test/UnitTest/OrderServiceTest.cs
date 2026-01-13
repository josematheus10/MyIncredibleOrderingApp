using Moq;
using Order.Api.Data.Repository;
using Order.Api.Data.UnitOfWork;
using Order.Api.Messaging.Events;
using Order.Api.Models;
using Order.Api.Services;

namespace Order.Api.Test.UnitTest
{
    public class OrderServiceTest
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderEvents> _mockOrderEvents;
        private readonly OrderService _orderService;

        public OrderServiceTest()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderEvents = new Mock<IOrderEvents>();

            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockUnitOfWork.Object,
                _mockOrderEvents.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrderSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 100.50m;
            var expectedOrderId = Guid.NewGuid();

            _mockOrderRepository
                .Setup(x => x.CreateOrderAsync(It.IsAny<Guid>(), customerId, totalValue, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrderId);

            // Act
            var result = await _orderService.CreateOrderAsync(customerId, totalValue);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockOrderRepository.Verify(x => x.CreateOrderAsync(It.IsAny<Guid>(), customerId, totalValue, It.IsAny<CancellationToken>()), Times.Once);
            _mockOrderEvents.Verify(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldPublishOrderCreatedEventWithCorrectData()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 250.75m;
            OrderCreatedEvent? capturedEvent = null;

            _mockOrderEvents
                .Setup(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<OrderCreatedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.CreateOrderAsync(customerId, totalValue);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(result, capturedEvent.OrderId);
            Assert.Equal(customerId, capturedEvent.CustomerId);
            Assert.Equal(totalValue, capturedEvent.TotalValue);
            Assert.Equal("CRIANDO", capturedEvent.Status);
            Assert.True((DateTime.UtcNow - capturedEvent.CreationDate).TotalSeconds < 2);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenRepositoryThrowsException_ShouldRollbackTransaction()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 100.50m;
            var expectedException = new InvalidOperationException("Database error");

            _mockOrderRepository
                .Setup(x => x.CreateOrderAsync(It.IsAny<Guid>(), customerId, totalValue, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(customerId, totalValue));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenPublishEventThrowsException_ShouldRollbackTransaction()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 100.50m;
            var expectedException = new InvalidOperationException("Event publishing error");

            _mockOrderEvents
                .Setup(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(customerId, totalValue));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenSaveChangesThrowsException_ShouldRollbackTransaction()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 100.50m;
            var expectedException = new InvalidOperationException("Save changes error");

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(customerId, totalValue));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void Constructor_WhenOrderRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new OrderService(null!, _mockUnitOfWork.Object, _mockOrderEvents.Object));

            Assert.Equal("orderRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new OrderService(_mockOrderRepository.Object, null!, _mockOrderEvents.Object));

            Assert.Equal("unitOfWork", exception.ParamName);
        }

        [Fact]
        public void Constructor_WhenOrderEventsIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new OrderService(_mockOrderRepository.Object, _mockUnitOfWork.Object, null!));

            Assert.Equal("orderCreatedPublisher", exception.ParamName);
        }

        [Fact]
        public async Task CreateOrderAsync_WithCancellationToken_ShouldPassTokenToAllMethods()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var totalValue = 100.50m;
            var cancellationToken = new CancellationToken();

            // Act
            await _orderService.CreateOrderAsync(customerId, totalValue, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(cancellationToken), Times.Once);
            _mockOrderRepository.Verify(x => x.CreateOrderAsync(It.IsAny<Guid>(), customerId, totalValue, cancellationToken), Times.Once);
            _mockOrderEvents.Verify(x => x.PublishOrderCreatedAsync(It.IsAny<OrderCreatedEvent>(), cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(cancellationToken), Times.Once);
        }
    }
}

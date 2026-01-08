using Microsoft.AspNetCore.Mvc;
using Order.Api.Models;
using Order.Api.Services;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [HttpPost(Name = "CreateOrder")]
        public async Task<IActionResult> Post([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var orderId = await _orderService.CreateOrderAsync(
                    request.CustomerId, 
                    request.TotalValue, 
                    cancellationToken);

                return Accepted(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Falha ao cria transação", Message = ex.Message });
            }
        }
    }
}

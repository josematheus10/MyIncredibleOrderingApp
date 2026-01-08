using Microsoft.AspNetCore.Mvc;
using Order.Api.Dto;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {

        [HttpPost(Name = "CreateOrder")]
        public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
        {
            var orderId = Guid.NewGuid();

            //queue.Enqueue(async ct =>
            //{
                // processamento pesado
                await Task.Delay(5000);
                Console.WriteLine($"Order {orderId} processed");
           // });

            return Accepted(new { OrderId = orderId } );
        }
    }
}

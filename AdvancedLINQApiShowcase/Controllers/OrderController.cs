using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedLINQApiShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            this._orderService = orderService;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        // Test the middleware.
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetOrder(int id)
        //{
        //    if (id <= 0)
        //        throw new ArgumentException("Invalid order ID provided.");

        //    var order = await _orderService.GetOrderByIdAsync(id);
        //    if (order == null)
        //        throw new KeyNotFoundException("Order not found.");

        //    return Ok(order);
        //}

        //// GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null || order.CustomerId == 0)
                return BadRequest(new
                {
                    error = "CustomerId is required."
                });

            await _orderService.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new
            {
                id = order.Id
            }, order);
        }


        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            if (order == null || id != order.Id)
                return BadRequest();
            await _orderService.UpdateOrderAsync(order);
            return NoContent();
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

    }
}

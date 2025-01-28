using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;
using AdvancedLINQApiShowcase.Services;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedLINQApiShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
    
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            this._orderService = orderService;
            this._logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching orders.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();
            return Ok(order);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            await _orderService.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new
            {
                id = order.Id
            }, order);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            if (id != order.Id)
            {
                return BadRequest(new
                {
                    error = "The ID in the URL does not match the ID in the request body."
                });
            }
            try
            {
                await _orderService.UpdateOrderAsync(order);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while updating the order. Please try again later."
                });
            }          
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while deleting the order. Please try again later."
                });
            }
        }

        [Authorize]
        [HttpGet("paged")]
        public async Task<IActionResult> GetOrdersAsync([FromQuery] PaginationFilter filter)
        {
            var result = await _orderService.GetOrdersAsync(filter);
            return Ok(result);
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

    }
}

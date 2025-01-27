using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;
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

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            _logger.LogInformation("Fetching all orders");

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
            _logger.LogInformation("Fetching order by id");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        // Keeping an extensive detailed log(CreateOrder)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            var transactionId = Guid.NewGuid().ToString(); // Generate a unique transaction ID for logging.
            _logger.LogInformation("Transaction {TransactionId} started: Creating an order.", transactionId);

            if (order == null || order.CustomerId == 0)
            {
                _logger.LogWarning("Transaction {TransactionId} failed: CustomerId is missing or invalid.", transactionId);
                return BadRequest(new
                {
                    error = "CustomerId is required."
                });
            }

            try
            {
                await _orderService.AddOrderAsync(order);
                _logger.LogInformation(
                    "Transaction {TransactionId} succeeded: Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}.",
                    transactionId, order.Id, order.CustomerId);

                return CreatedAtAction(nameof(GetOrder), new
                {
                    id = order.Id
                }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Transaction {TransactionId} failed: An error occurred while creating the order. CustomerId: {CustomerId}.",
                    transactionId, order.CustomerId);

                return StatusCode(500, new
                {
                    error = "An unexpected error occurred. Please try again later."
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            _logger.LogInformation("Updated order");

            if (order == null || id != order.Id)
                return BadRequest();
            await _orderService.UpdateOrderAsync(order);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation("Deleted order");

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetOrdersAsync([FromQuery] PaginationFilter filter)
        {
            var result = await _orderService.GetOrdersAsync(filter);
            return Ok(result);
        }


    }
}

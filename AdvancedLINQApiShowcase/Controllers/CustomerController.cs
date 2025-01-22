using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedLINQApiShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            this._customerService = customerService;
            this._logger = logger;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            _logger.LogInformation("Fetching all customers");

            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Customer
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            _logger.LogInformation("Fetching customer by id");

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return NotFound();
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            _logger.LogInformation("Created customer");


            if (customer == null)
                return BadRequest();

            await _customerService.AddCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new
            {
                id = customer.Id
            }, customer);
        }

        // PUT: api/Customer
        // Keeping an extensive detailed log(UpdateCustomer)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            var transactionId = Guid.NewGuid().ToString(); // Unique transaction ID for this request.
            _logger.LogInformation("Transaction {TransactionId} started: Updating a customer. CustomerId: {CustomerId}", transactionId, id);

            if (id != customer.Id)
            {
                _logger.LogWarning("Transaction {TransactionId} failed: Mismatched CustomerId in URL ({UrlId}) and body ({BodyId}).", transactionId, id, customer.Id);
                return BadRequest(new
                {
                    error = "The ID in the URL does not match the ID in the request body."
                });
            }

            try
            {
                customer.Id = id;
                await _customerService.UpdateCustomerAsync(customer);
                _logger.LogInformation(
                    "Transaction {TransactionId} succeeded: Customer updated successfully. CustomerId: {CustomerId}, Name: {CustomerName}",
                    transactionId, customer.Id, customer.Name);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Transaction {TransactionId} failed: An error occurred while updating the customer. CustomerId: {CustomerId}, Name: {CustomerName}",
                    transactionId, customer.Id, customer.Name);

                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while updating the customer. Please try again later."
                });
            }
        }


        // DELETE: api/Customer
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _logger.LogInformation("Deleted customer");

            await _customerService.DeleteCustomerByIdAsync(id);      
            return NoContent();
        }
    }
}

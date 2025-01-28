using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;
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

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching customers.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer is null)
                return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            //if (customer == null)
            //    return BadRequest();
            await _customerService.AddCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new
            {
                id = customer.Id
            }, customer);
        }
       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest(new
                {
                    error = "The ID in the URL does not match the ID in the request body."
                });
            }
            try
            {
                await _customerService.UpdateCustomerAsync(customer);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while updating the customer. Please try again later."
                });
            }          
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteCustomerByIdAsync(id);
                return NoContent();
            }
            catch (Exception)
            {               
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while deleting the customer. Please try again later."
                });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetCustomers([FromQuery] PaginationFilter filter)
        {
            var result = await _customerService.GetCustomerAsync(filter);
            return Ok(result);
        }

    }
}

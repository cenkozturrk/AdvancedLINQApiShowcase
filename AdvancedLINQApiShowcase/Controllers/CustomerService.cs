using AdvancedLINQApiShowcase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedLINQApiShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerService : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerService(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCostomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomersByIdAsync(id);

            return Ok(customer);
        }
    }
}

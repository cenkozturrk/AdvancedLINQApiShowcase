using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.Include(c => c.Orders).ToListAsync();
        }

        public async Task<Customer> GetCustomersByIdAsync(int id)
        {
            return await _context.Customers.Include(c => c.Orders)
                                           .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

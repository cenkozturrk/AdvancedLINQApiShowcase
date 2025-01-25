using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AdvancedLINQApiShowcase.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;


        public CustomerService(AppDbContext context, ICacheService cache)
        {
            this._context = context;
            this._cache = cache;
        }
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            const string cacheKey = "GetAllCustomers";

            var cachedCustomers = _cache.GetData<IEnumerable<Customer>>(cacheKey);

            if (cachedCustomers is not null)
            {
                return cachedCustomers;
            }

            var customers = await _context.Customers.ToListAsync();

            if (customers.Any())
            {
                _cache.SetData(cacheKey, customers);
            }
            return customers;
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            string cacheKey = $"Customer_{id}"; 
           
            var cachedCustomer = _cache.GetData<Customer>(cacheKey);

            if (cachedCustomer is not null)
            {
                return cachedCustomer; 
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

            if (customer is not null)
            {
                _cache.SetData(cacheKey, customer);
            }
            return customer;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();            
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.Id);
            if (existingCustomer != null)
            {
                existingCustomer.Name = customer.Name;
                existingCustomer.Email = customer.Email;
                await _context.SaveChangesAsync();
            }                
        }
        public async Task DeleteCustomerByIdAsync(int id)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer != null)
            {
                _context.Customers.Remove(existingCustomer);
                await _context.SaveChangesAsync();
            }
        }

    }
}

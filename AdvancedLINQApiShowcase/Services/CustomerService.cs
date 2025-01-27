using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AdvancedLINQApiShowcase.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(AppDbContext context, ICacheService cache, ILogger<CustomerService> logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            try
            {
                const string cacheKey = "AllCustomers";

                var cachedCustomers = _cache.GetData<IEnumerable<Customer>>(cacheKey);

                if (cachedCustomers is not null)
                {
                    _logger.LogInformation("Cache hit for all customers.");
                    return cachedCustomers;
                }

                _logger.LogInformation("Cache miss for all customers. Retrieving from database.");

                var customers = await _context.Customers.ToListAsync();

                if (customers.Any())
                {
                    _cache.SetData(cacheKey, customers);
                }
                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all customers.");
                throw;
            }
          
        }
        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"Customer_{id}";
                var cachedCustomer = _cache.GetData<Customer>(cacheKey);

                if (cachedCustomer is not null)
                {
                    _logger.LogInformation($"Cache hit for customer {id}.");
                    return cachedCustomer;
                }

                _logger.LogInformation($"Cache miss for customer {id}. Retrieving from database.");
                var customer = await _context.Customers.FindAsync(id);

                if (customer is not null)
                {
                    _cache.SetData(cacheKey, customer);
                }
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the customer with ID {id}.");
                throw;
            }
            
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
        public async Task<PaginatedResult<Customer>> GetCustomerAsync(PaginationFilter filter)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchQuery))            
                query = query.Where(c => c.Name.Contains(filter.SearchQuery));

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var propertyInfo = typeof(Customer).GetProperty(filter.SortBy);
                if (propertyInfo != null)                
                    query = filter.IsDescending
                        ? query.OrderByDescending(c => EF.Property<object>(c, filter.SortBy))
                        : query.OrderBy(c => EF.Property<object>(c, filter.SortBy));                
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResult<Customer>()
            {
                Data = data,
                TotalRecords = totalCount,
                PageSize = filter.PageSize,
                CurrentPage = filter.PageNumber,
            };
        }
    }
}

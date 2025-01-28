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
            _logger.LogInformation("Fetching all customers from the database");

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

                var customers = await _context.Customers
                    .OrderBy(c => c.Id)
                    .ToListAsync();

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
            _logger.LogInformation("Fetching customer by id");

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
            _logger.LogInformation("Adding a new Customer.");

            try
            {
                if (customer is null)
                {
                    _logger.LogInformation("a new customer could not be created because the user did not enter customer information.");

                    throw new ArgumentNullException(nameof(customer), "Customer cannot be null.");
                }
                _logger.LogInformation("Adding new customer with name: {CustomerName}", customer.Name);

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer with ID {CustomerId} created successfully.", customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the customer.");
                throw;
            }
                             
        }
        public async Task UpdateCustomerAsync(Customer customer)
        {
            _logger.LogInformation("Updating existing Customer");

            var existingCustomer = await _context.Customers.FindAsync(customer.Id);
            if (existingCustomer is not null)
            {
                _logger.LogInformation("Updating customer with ID {CustomerId} and Name {CustomerName}", customer.Id, customer.Name);

                existingCustomer.Name = customer.Name;
                existingCustomer.Email = customer.Email;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated customer with ID {CustomerId}.", customer.Id);
            }
            else
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", customer.Id);
                throw new KeyNotFoundException($"Customer with ID {customer.Id} not found.");
            }
        }
        public async Task DeleteCustomerByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to delete customer with ID: {CustomerId}", id);
            var transactionId = Guid.NewGuid().ToString();

            _logger.LogInformation("Transaction {TransactionId} started: Deleting customer with ID: {CustomerId}", transactionId, id);
            try
            {
                var existingCustomer = await _context.Customers.FindAsync(id);
                if (existingCustomer is not null)
                {
                    _logger.LogInformation("Customer found: Deleting customer with ID {CustomerId}", id);

                    _context.Customers.Remove(existingCustomer);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Transaction {TransactionId} succeeded: Customer with ID {CustomerId} deleted successfully.", transactionId, id);

                    _logger.LogInformation("Customer with ID {CustomerId} deleted successfully.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction {TransactionId} failed: An error occurred while deleting the customer with ID {CustomerId}.", transactionId, id);

                _logger.LogWarning("Customer with ID {CustomerId} not found. Nothing to delete.", id);
            }         
          
        }
        public async Task<PaginatedResult<Customer>> GetCustomerAsync(PaginationFilter filter)
        {
            _logger.LogInformation("Fetching all customers from the database");

            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchQuery))            
                query = query.Where(c => c.Name.Contains(filter.SearchQuery));
            
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var propertyInfo = typeof(Customer).GetProperty(filter.SortBy);
                if (propertyInfo is not null)                
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

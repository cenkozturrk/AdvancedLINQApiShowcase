using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;
        private readonly ILogger<OrderService> _logger;
        public OrderService(AppDbContext context, ICacheService cache, ILogger<OrderService> logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Fetching all orders from the database");

            try
            {
                const string cacheKey = "AllOrders";
                var cachedOrders = _cache.GetData<IEnumerable<Order>>(cacheKey);

                if (cachedOrders is not null)
                {
                    _logger.LogInformation("Cache hit for all orders.");
                    return cachedOrders;
                }

                _logger.LogInformation("Cache miss for all orders. Retrieving from database.");

                var orders = await _context.Orders
                    .OrderBy(c =>c.Id)
                    .ToListAsync();

                if (orders.Any())
                {
                    _cache.SetData(cacheKey, orders);
                }
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all orders.");
                throw;
            }
        }
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            _logger.LogInformation("Fetching order by id");

            try
            {
                var cacheKey = $"Order_{id}";
                var cachedOrder = _cache.GetData<Order>(cacheKey);

                if (cachedOrder is not null)
                {
                    _logger.LogInformation($"Cache hit for order {id}.");
                    return cachedOrder;
                }

                _logger.LogInformation($"Cache miss for order {id}. Retrieving from database.");
                var order = await _context.Orders.FindAsync(id);

                if (order is not null)
                {
                    _cache.SetData(cacheKey, order);
                }
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the order with ID {id}.");
                throw;
            }
        }
        public async Task AddOrderAsync(Order order)
        {
            _logger.LogInformation("Adding a new Order.");

            try
            {
                if (order is null)
                {
                    _logger.LogInformation("a new order could not be created because the user did not enter order information.");

                    throw new ArgumentNullException(nameof(order), "Order cannot be null.");
                }
                _logger.LogInformation("Adding new order with name: {OrderName}", order.Name);

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order with ID {OrderId} created successfully.", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the order.");
                throw;
            }
        }
        public async Task UpdateOrderAsync(Order order)
        {
            _logger.LogInformation("Updating existing Order");

            var existingOrderToUpdate = await _context.Orders.FindAsync(order.Id);
            if (existingOrderToUpdate is not null)
            {
                _logger.LogInformation("Updating order with ID {OrderId} and Name {OrderName}", order.Id, order.Name);

                existingOrderToUpdate.Name = order.Name;
                existingOrderToUpdate.OrderDate = order.OrderDate;
                existingOrderToUpdate.CustomerId = order.CustomerId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated order with ID {OrderId}.", order.Id);

            }
        }
        public async Task DeleteOrderAsync(int id)
        {
            _logger.LogInformation("Attempting to delete order with ID: {OrderId}", id);
            var transactionId = Guid.NewGuid().ToString();

            _logger.LogInformation("Transaction {TransactionId} started: Deleting order with ID: {OrderId}", transactionId, id);
            try
            {
                var existingOrder = await _context.Orders.FindAsync(id);
                if (existingOrder is not null)
                {
                    _logger.LogInformation("Order found: Deleting order with ID {OrderId}", id);

                    _context.Orders.Remove(existingOrder);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Transaction {TransactionId} succeeded: Order with ID {OrderId} deleted successfully.", transactionId, id);

                    _logger.LogInformation("Order with ID {OrderId} deleted successfully.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction {TransactionId} failed: An error occurred while deleting the order with ID {OrderId}.", transactionId, id);

                _logger.LogWarning("Order with ID {OrderId} not found. Nothing to delete.", id);
            }
        }
        public async Task<PaginatedResult<Order>> GetOrdersAsync(PaginationFilter filter)
        {
            _logger.LogInformation("Fetching all orders from the database");

            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(o => o.Name.Contains(filter.SearchQuery));


            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var propertyInfo = typeof(Order).GetProperty(filter.SortBy);
                if (propertyInfo is not null)
                    query = filter.IsDescending
                    ? query.OrderByDescending(e => EF.Property<object>(e, filter.SortBy))
                    : query.OrderBy(e => EF.Property<object>(e, filter.SortBy));

            }
            var totalRecords = await query.CountAsync();

            var data = await query
               .Skip((filter.PageNumber - 1) * filter.PageSize)
               .Take(filter.PageSize)
               .ToListAsync();

            return new PaginatedResult<Order>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageSize = filter.PageSize,
                CurrentPage = filter.PageNumber
            };
        }
    }
}

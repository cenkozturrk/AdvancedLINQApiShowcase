using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;
        private readonly ILogger<OrderService> _logger;


        public OrderService(AppDbContext context,ICacheService cache,ILogger<OrderService> logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
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
                var orders = await _context.Orders.ToListAsync();

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
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateOrderAsync(Order order)
        {
            var existingOrderToUpdate = await _context.Orders.FindAsync(order.Id);
            if (existingOrderToUpdate != null)
            {
                existingOrderToUpdate.Name = order.Name;
                existingOrderToUpdate.OrderDate = order.OrderDate;
                existingOrderToUpdate.CustomerId = order.CustomerId;
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteOrderAsync(int id)
        {
            var orderDelete = await _context.Orders.FindAsync(id);
            if (orderDelete != null)
            {
                _context.Orders.Remove(orderDelete);
                await _context.SaveChangesAsync();
            }
        }

        
    }
}

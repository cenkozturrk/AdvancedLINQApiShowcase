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


        public OrderService(AppDbContext context,ICacheService cache)
        {
            this._context = context;
            this._cache = cache;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            const string cacheKey = "GetAllOrders";
            
            var cachedOrders = _cache.GetData<IEnumerable<Order>>(cacheKey);

            if (cachedOrders is not null)
            {
                return cachedOrders; 
            }           
            var orders = await _context.Orders.ToListAsync();

            if (orders.Any())
            {
                _cache.SetData(cacheKey, orders);
            }
            return orders;
        }


        public async Task<Order> GetOrderByIdAsync(int id)
        {
            string cacheKey = $"Order_{id}"; 

            var cachedOrder = _cache.GetData<Order>(cacheKey);

            if (cachedOrder is not null)
            {
                return cachedOrder; 
            }                      

            var order = await _context.Orders.FindAsync(id);

            if (order is not null)
            {               
                _cache.SetData(cacheKey, order);
            }
            return order;
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

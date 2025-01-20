using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
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

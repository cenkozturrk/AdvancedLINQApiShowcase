using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;

namespace AdvancedLINQApiShowcase.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task<PaginatedResult<Order>> GetOrdersAsync(PaginationFilter filter);

    }
}

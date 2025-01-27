using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Pagination;

namespace AdvancedLINQApiShowcase.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(int id);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerByIdAsync(int id);
        Task<PaginatedResult<Customer>> GetCustomerAsync(PaginationFilter filter);
    }
}

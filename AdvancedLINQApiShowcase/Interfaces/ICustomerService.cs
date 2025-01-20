using AdvancedLINQApiShowcase.Models;

namespace AdvancedLINQApiShowcase.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomersByIdAsync(int id);
    }
}

using AdvancedLINQApiShowcase.Dto;
using AdvancedLINQApiShowcase.Models;

namespace AdvancedLINQApiShowcase.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);
    }
}

using AdvancedLINQApiShowcase.Dto;
using AdvancedLINQApiShowcase.Dtos;
using AdvancedLINQApiShowcase.Models;

namespace AdvancedLINQApiShowcase.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}

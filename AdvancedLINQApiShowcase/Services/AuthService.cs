using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Dto;
using AdvancedLINQApiShowcase.Interfaces;
using AdvancedLINQApiShowcase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null;
            }

            User user = new User();

            var hashedPassword = new PasswordHasher<User>()
                 .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }
        public Task<string?> LoginAsync(UserDto request)
        {
            throw new NotImplementedException();
        }

       
    }
}

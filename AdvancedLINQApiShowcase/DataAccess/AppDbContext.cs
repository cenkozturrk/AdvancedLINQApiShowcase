using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}

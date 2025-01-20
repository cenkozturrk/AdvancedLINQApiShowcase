using AdvancedLINQApiShowcase.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedLINQApiShowcase.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        
    }



}

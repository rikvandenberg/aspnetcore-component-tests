using Microsoft.EntityFrameworkCore;

namespace Api.DataLayer
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> contextOptions)
            : base(contextOptions)
        {
        }

        public DbSet<Order> Order => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    }
}
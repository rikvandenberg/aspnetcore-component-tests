using Microsoft.EntityFrameworkCore;

namespace Api.DataLayer
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> contextOptions)
            : base(contextOptions)
        {
        }

        public DbSet<Order> Order { get; set; }

        public DbSet<OrderLine> OrderLines { get; set; }
    }
}
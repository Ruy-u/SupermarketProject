using Microsoft.EntityFrameworkCore;
using SupermarketProject.Models;

namespace SupermarketProject.Data
{
    public class SupermarketDbContext : DbContext
    {
        public SupermarketDbContext(DbContextOptions<SupermarketDbContext> options)
            : base(options)
        {
        }

        // DbSets for each model
        public DbSet<UserAccountProjects> UserAccountProjects { get; set; }
        public DbSet<CustomerProjects> CustomerProjects { get; set; }
        public DbSet<ItemProjects> ItemProjects { get; set; }
        public DbSet<OrderProjects> OrderProjects { get; set; }
        public DbSet<OrderLineProjects> OrderLineProjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderLineProjects>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLineProjects)
                .HasForeignKey(ol => ol.OrderId);
        }
    }
}

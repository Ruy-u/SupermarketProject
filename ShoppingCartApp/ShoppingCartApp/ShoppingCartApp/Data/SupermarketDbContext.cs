using Microsoft.EntityFrameworkCore;
using ShoppingCartApp.Models;

namespace ShoppingCartApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet properties for your tables
        public DbSet<UserAccountProjects> UserAccounts { get; set; }
        public DbSet<CustomerProjects> Customers { get; set; }
        public DbSet<ItemProjects> Items { get; set; }
        public DbSet<OrderProjects> Orders { get; set; }
        public DbSet<OrderLineProjects> OrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UserAccountProjects table mapping
            modelBuilder.Entity<UserAccountProjects>(entity =>
            {
                entity.ToTable("UserAccountProjects");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Pass).HasColumnName("pass");
                entity.Property(e => e.Role).HasColumnName("role");
            });

            // CustomerProjects table mapping
            modelBuilder.Entity<CustomerProjects>(entity =>
            {
                entity.ToTable("CustomerProjects");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Job).HasColumnName("job");
                entity.Property(e => e.Married).HasColumnName("married");
                entity.Property(e => e.Gender).HasColumnName("gender");
                entity.Property(e => e.Location).HasColumnName("location");
            });

            // ItemProjects table mapping
            modelBuilder.Entity<ItemProjects>(entity =>
            {
                entity.ToTable("ItemProjects");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Discount).HasColumnName("discount");
                entity.Property(e => e.Category).HasColumnName("category");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Imgfile).HasColumnName("imgfile");
            });

            // Configure OrderProjects table
            modelBuilder.Entity<OrderProjects>(entity =>
            {
                entity.ToTable("OrderProjects");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustName).HasColumnName("custname");
                entity.Property(o => o.OrderDate).HasColumnName("orderdate");
                entity.Property(o => o.Total).HasColumnName("total");

                entity.HasMany(o => o.OrderLineProjects)
                      .WithOne(ol => ol.Order)
                      .HasForeignKey(ol => ol.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderLineProjects table
            modelBuilder.Entity<OrderLineProjects>(entity =>
            {
                entity.ToTable("OrderLineProjects");
                entity.HasKey(ol => ol.Id);
                entity.Property(ol => ol.ItemName).HasColumnName("itemname");
                entity.Property(ol => ol.ItemQuant).HasColumnName("itemquant");
                entity.Property(ol => ol.ItemPrice).HasColumnName("itemprice");
                entity.Property(ol => ol.OrderId).HasColumnName("orderid");
            });
        }
    }
}

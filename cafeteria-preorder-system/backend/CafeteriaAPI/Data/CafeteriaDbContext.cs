using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CafeteriaAPI.Models;

namespace CafeteriaAPI.Data
{
    public class CafeteriaDbContext : DbContext
    {
        public CafeteriaDbContext(DbContextOptions<CafeteriaDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names (optional - PostgreSQL lowercase with underscores)
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<MenuItem>().ToTable("menu_items");
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderItem>().ToTable("order_items");

            // Configure owned types
            modelBuilder.Entity<MenuItem>().OwnsOne(m => m.NutritionInfo);

            // Configure JSON columns for lists
            modelBuilder.Entity<User>()
                .Property(u => u.DietaryPreferences)
                .HasColumnType("text[]");

            modelBuilder.Entity<User>()
                .Property(u => u.Allergies)
                .HasColumnType("text[]");

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.DietaryTags)
                .HasColumnType("text[]");

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Allergens)
                .HasColumnType("text[]");
        }
    }
}

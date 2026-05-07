using Microsoft.EntityFrameworkCore;
using MenuService.Models;

namespace MenuService.Data;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }

    public DbSet<MenuItem> MenuItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Available);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Available).HasDefaultValue(true);
            entity.Property(e => e.OrdersToday).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

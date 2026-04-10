using MDator.Samples.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MDator.Samples.WebApi.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Sku).HasMaxLength(50);
            e.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<StockAlert>(e =>
        {
            e.HasKey(a => a.Id);
        });
    }
}

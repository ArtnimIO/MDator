using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MDator.Samples.WebApi.Infrastructure.Repositories;

public class EfProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Products.FindAsync([id], ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(int skip = 0, int take = 50, CancellationToken ct = default)
        => await db.Products.OrderBy(p => p.Name).Skip(skip).Take(take).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default)
        => await db.Products.Where(p => p.Name.Contains(term)).OrderBy(p => p.Name).ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        db.Products.Update(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is not null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync(ct);
        }
    }
}

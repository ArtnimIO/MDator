using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MDator.Samples.WebApi.Infrastructure.Repositories;

public class EfCategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Categories.FindAsync([id], ct);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => await db.Categories.OrderBy(c => c.Name).ToListAsync(ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
    }
}

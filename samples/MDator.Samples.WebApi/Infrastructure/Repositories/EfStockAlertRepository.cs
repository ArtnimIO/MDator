using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MDator.Samples.WebApi.Infrastructure.Repositories;

public class EfStockAlertRepository(AppDbContext db) : IStockAlertRepository
{
    public async Task AddAsync(StockAlert alert, CancellationToken ct = default)
    {
        db.StockAlerts.Add(alert);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StockAlert>> GetByProductAsync(Guid productId, CancellationToken ct = default)
        => await db.StockAlerts.Where(a => a.ProductId == productId)
            .OrderByDescending(a => a.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<StockAlert>> GetRecentAsync(int count = 20, CancellationToken ct = default)
        => await db.StockAlerts.OrderByDescending(a => a.CreatedAt).Take(count).ToListAsync(ct);
}

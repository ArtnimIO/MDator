using MDator.Samples.Domain.Models;

namespace MDator.Samples.Domain.Interfaces;

public interface IStockAlertRepository
{
    Task AddAsync(StockAlert alert, CancellationToken ct = default);
    Task<IReadOnlyList<StockAlert>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<StockAlert>> GetRecentAsync(int count = 20, CancellationToken ct = default);
}

using System.Collections.Concurrent;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.Console.Infrastructure;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _store = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task<IReadOnlyList<Product>> GetAllAsync(int skip = 0, int take = 50, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Product>>(_store.Values.Skip(skip).Take(take).ToList());

    public Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Product>>(
            _store.Values.Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList());

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _store[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _store[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly ConcurrentDictionary<Guid, Category> _store = new();

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Category>>(_store.Values.ToList());

    public Task AddAsync(Category category, CancellationToken ct = default)
    {
        _store[category.Id] = category;
        return Task.CompletedTask;
    }
}

public class InMemoryStockAlertRepository : IStockAlertRepository
{
    private readonly List<StockAlert> _store = [];

    public Task AddAsync(StockAlert alert, CancellationToken ct = default)
    {
        _store.Add(alert);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StockAlert>> GetByProductAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<StockAlert>>(_store.Where(a => a.ProductId == productId).ToList());

    public Task<IReadOnlyList<StockAlert>> GetRecentAsync(int count = 20, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<StockAlert>>(
            _store.OrderByDescending(a => a.CreatedAt).Take(count).ToList());
}

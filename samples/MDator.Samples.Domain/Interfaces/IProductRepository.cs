using MDator.Samples.Domain.Models;

namespace MDator.Samples.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(int skip = 0, int take = 50, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the product tracked by the context so that updates can be persisted.
    /// </summary>
    Task<Product?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByStockRangeAsync(int min, int max, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    void Remove(Product product);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

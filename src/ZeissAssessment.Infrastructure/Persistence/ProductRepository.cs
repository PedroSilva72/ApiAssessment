using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Product?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken) =>
        await context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken)
    {
        var pattern = $"%{name}%";
        return await context.Products.AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, pattern))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStockRangeAsync(int min, int max, CancellationToken cancellationToken) =>
        await context.Products.AsNoTracking()
            .Where(p => p.Stock >= min && p.Stock <= max)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await context.Products.AddAsync(product, cancellationToken);

    public void Remove(Product product) => context.Products.Remove(product);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}

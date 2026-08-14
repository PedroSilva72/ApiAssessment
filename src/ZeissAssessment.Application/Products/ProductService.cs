using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Domain.Entities;
using ZeissAssessment.Domain.Exceptions;

namespace ZeissAssessment.Application.Products;

public class ProductService(IProductRepository repository, IProductIdGenerator idGenerator)
    : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        return Map(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = await idGenerator.NextAsync(cancellationToken),
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
        };

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        product.Name = request.Name.Trim();
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await repository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        repository.Remove(product);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductResponse> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        product.DecrementStock(quantity);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> AddToStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
        product.IncrementStock(quantity);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> SearchByNameAsync(string name, CancellationToken cancellationToken)
    {
        var products = await repository.SearchByNameAsync(name ?? string.Empty, cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetByStockLevelAsync(int min, int max, CancellationToken cancellationToken)
    {
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min));
        if (max < min) throw new ArgumentException("Max must be greater than or equal to min.", nameof(max));

        var products = await repository.GetByStockRangeAsync(min, max, cancellationToken);
        return products.Select(Map).ToList();
    }

    private static ProductResponse Map(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock);
}

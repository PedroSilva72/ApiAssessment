namespace ZeissAssessment.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

    Task<ProductResponse> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken);

    Task<ProductResponse> AddToStockAsync(int id, int quantity, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> SearchByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> GetByStockLevelAsync(int min, int max, CancellationToken cancellationToken);
}

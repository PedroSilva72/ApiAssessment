namespace ZeissAssessment.Application.Products;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock);

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock);

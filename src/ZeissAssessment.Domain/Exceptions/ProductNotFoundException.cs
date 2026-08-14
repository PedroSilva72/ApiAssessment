namespace ZeissAssessment.Domain.Exceptions;

public class ProductNotFoundException(int productId) : Exception($"Product with id '{productId}' was not found.")
{
    public int ProductId { get; } = productId;
}

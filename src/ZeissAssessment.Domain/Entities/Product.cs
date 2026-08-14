namespace ZeissAssessment.Domain.Entities;

/// <summary>
/// Represents a product managed by the system.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique 6-digit identifier assigned by the persistence layer via a database sequence.
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    /// <summary>
    /// Current available stock.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Concurrency token used to detect concurrent stock modifications.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void DecrementStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (Stock < quantity)
        {
            throw new InvalidStockOperationException(
                $"Cannot decrement stock by {quantity}. Available stock is {Stock}.");
        }

        Stock -= quantity;
    }

    public void IncrementStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Stock += quantity;
    }
}

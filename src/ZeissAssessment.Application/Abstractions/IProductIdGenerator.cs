namespace ZeissAssessment.Application.Abstractions;

/// <summary>
/// Generates unique 6-digit product identifiers. Implementations must guarantee uniqueness
/// even when multiple API instances run concurrently (typically backed by a database sequence).
/// </summary>
public interface IProductIdGenerator
{
    Task<int> NextAsync(CancellationToken cancellationToken);
}

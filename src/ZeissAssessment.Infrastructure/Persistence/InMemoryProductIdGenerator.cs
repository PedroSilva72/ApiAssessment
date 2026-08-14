using ZeissAssessment.Application.Abstractions;

namespace ZeissAssessment.Infrastructure.Persistence;

/// <summary>
/// Fallback in-memory generator for providers that don't support SQL sequences (e.g. SQLite in tests).
/// Thread-safe within a single process only.
/// </summary>
public class InMemoryProductIdGenerator(int start = 100_000) : IProductIdGenerator
{
    private int _current = start - 1;

    public Task<int> NextAsync(CancellationToken cancellationToken)
    {
        var next = Interlocked.Increment(ref _current);
        if (next > 999_999)
        {
            throw new InvalidOperationException("Product id sequence exhausted.");
        }
        return Task.FromResult(next);
    }
}

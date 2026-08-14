using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Application.Abstractions;

namespace ZeissAssessment.Infrastructure.Persistence;

/// <summary>
/// Generates unique 6-digit product ids by pulling values from a SQL Server SEQUENCE.
/// SQL Server guarantees uniqueness of sequence values across concurrent sessions.
/// </summary>
public class SqlServerProductIdGenerator(ProductDbContext context) : IProductIdGenerator
{
    public async Task<int> NextAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT NEXT VALUE FOR [{ProductDbContext.ProductIdSequenceName}];";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}

using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public const string ProductIdSequenceName = "ProductIdSequence";

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Database.IsSqlServer())
        {
            modelBuilder.HasSequence<int>(ProductIdSequenceName)
                .StartsAt(100_000)
                .IncrementsBy(1)
                .HasMin(100_000)
                .HasMax(999_999);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);

        // SQLite (used in tests) doesn't auto-generate rowversion values, so provide a
        // default to satisfy the NOT NULL constraint on inserts. SQL Server rejects
        // defaults on rowversion columns, so this branch only runs elsewhere.
        if (!Database.IsSqlServer())
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .HasDefaultValue(Array.Empty<byte>());
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Id is assigned explicitly from the sequence at insert time.
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Stock).IsRequired();

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasIndex(p => p.Name);

        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Id_SixDigits",
            "[Id] >= 100000 AND [Id] <= 999999"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Stock_NonNegative", "[Stock] >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0"));
    }
}

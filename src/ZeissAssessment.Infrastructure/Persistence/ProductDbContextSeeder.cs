using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence;

public static class ProductDbContextSeeder
{
    public static async Task SeedAsync(ProductDbContext context, IProductIdGenerator idGenerator, CancellationToken cancellationToken = default)
    {
        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var seedData = new (string Name, string Description, decimal Price, int Stock)[]
        {
            ("Widget",       "Standard widget",             9.99m,   100),
            ("Gadget",       "Multi-purpose gadget",        19.99m,  50),
            ("Sprocket",     "High-precision sprocket",     4.50m,   250),
            ("Cog",          "Replacement cog",             2.75m,   500),
            ("Flux Capacitor","Time-travel component",     1999.00m, 3),
        };

        foreach (var item in seedData)
        {
            context.Products.Add(new Product
            {
                Id = await idGenerator.NextAsync(cancellationToken),
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

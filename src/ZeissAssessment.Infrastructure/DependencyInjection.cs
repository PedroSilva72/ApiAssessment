using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductDb")
            ?? throw new InvalidOperationException("Connection string 'ProductDb' not configured.");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductIdGenerator, SqlServerProductIdGenerator>();

        return services;
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.Tests.Integration;

public class ProductApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ProductApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core registrations to avoid mixed provider errors.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ProductDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") ?? false)
            ).ToList();
            foreach (var d in toRemove) services.Remove(d);

            var idGenDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IProductIdGenerator));
            if (idGenDescriptor is not null) services.Remove(idGenDescriptor);

            services.AddDbContext<ProductDbContext>(options => options.UseSqlite(_connection));
            services.AddSingleton<IProductIdGenerator, InMemoryProductIdGenerator>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}


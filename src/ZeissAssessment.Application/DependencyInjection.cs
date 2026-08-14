using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Application.Products;
// AddValidatorsFromAssemblyContaining lives in FluentValidation namespace (DI extensions package).

namespace ZeissAssessment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        return services;
    }
}

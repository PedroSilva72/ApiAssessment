using Microsoft.EntityFrameworkCore;
using ZeissAssessment.Api.Infrastructure;
using ZeissAssessment.Application;
using ZeissAssessment.Application.Abstractions;
using ZeissAssessment.Infrastructure;
using ZeissAssessment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
});

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed initial data.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    var idGenerator = scope.ServiceProvider.GetRequiredService<IProductIdGenerator>();
    if (context.Database.IsSqlServer())
    {
        await context.Database.MigrateAsync();
    }
    else
    {
        await context.Database.EnsureCreatedAsync();
    }
    await ProductDbContextSeeder.SeedAsync(context, idGenerator);
}

app.Run();

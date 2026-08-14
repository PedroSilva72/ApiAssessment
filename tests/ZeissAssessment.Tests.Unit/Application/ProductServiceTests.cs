using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ZeissAssessment.Application.Products;
using ZeissAssessment.Domain.Exceptions;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.Tests.Unit.Application;

public class ProductServiceTests : IAsyncLifetime
{
    private ProductDbContext _context = null!;
    private ProductService _service = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlite($"DataSource=file:mem-{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
        _context = new ProductDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        var repo = new ProductRepository(_context);
        _service = new ProductService(repo, new InMemoryProductIdGenerator());
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _context.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_AssignsSixDigitId()
    {
        var created = await _service.CreateAsync(new CreateProductRequest("A", null, 1m, 5), default);
        created.Id.Should().BeInRange(100_000, 999_999);
        created.Stock.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenMissing()
    {
        var act = () => _service.GetByIdAsync(999999, default);
        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task DecrementStock_UpdatesValue()
    {
        var created = await _service.CreateAsync(new CreateProductRequest("A", null, 1m, 10), default);
        var result = await _service.DecrementStockAsync(created.Id, 3, default);
        result.Stock.Should().Be(7);
    }

    [Fact]
    public async Task SearchByName_PartialMatch_ReturnsResults()
    {
        await _service.CreateAsync(new CreateProductRequest("Widget", null, 1m, 1), default);
        await _service.CreateAsync(new CreateProductRequest("Gadget", null, 1m, 1), default);
        var results = await _service.SearchByNameAsync("widg", default);
        results.Should().ContainSingle(r => r.Name == "Widget");
    }

    [Fact]
    public async Task GetByStockLevel_ReturnsInRange()
    {
        await _service.CreateAsync(new CreateProductRequest("A", null, 1m, 5), default);
        await _service.CreateAsync(new CreateProductRequest("B", null, 1m, 50), default);
        await _service.CreateAsync(new CreateProductRequest("C", null, 1m, 500), default);

        var results = await _service.GetByStockLevelAsync(10, 100, default);
        results.Should().ContainSingle(r => r.Name == "B");
    }
}

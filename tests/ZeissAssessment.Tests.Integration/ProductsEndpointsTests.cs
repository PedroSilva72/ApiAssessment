using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using ZeissAssessment.Application.Products;

namespace ZeissAssessment.Tests.Integration;

public class ProductsEndpointsTests(ProductApiFactory factory) : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsSeededOrEmpty()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_Get_Update_Delete_FullCycle()
    {
        // Create
        var createResp = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("IntegrationProduct", "desc", 9.99m, 20));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ProductResponse>();
        created!.Id.Should().BeInRange(100_000, 999_999);

        // Get by id
        var getResp = await _client.GetAsync($"/api/products/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        var updResp = await _client.PutAsJsonAsync($"/api/products/{created.Id}",
            new UpdateProductRequest("IntegrationProductV2", "desc", 12.50m, 25));
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updResp.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.Name.Should().Be("IntegrationProductV2");

        // Decrement
        var dec = await _client.PostAsync($"/api/products/{created.Id}/decrement-stock/5", null);
        dec.StatusCode.Should().Be(HttpStatusCode.OK);

        // Add
        var add = await _client.PostAsync($"/api/products/{created.Id}/add-to-stock/2", null);
        add.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterAdd = await add.Content.ReadFromJsonAsync<ProductResponse>();
        afterAdd!.Stock.Should().Be(25 - 5 + 2);

        // Delete
        var del = await _client.DeleteAsync($"/api/products/{created.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var missing = await _client.GetAsync($"/api/products/{created.Id}");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Invalid_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("", null, -1m, -5));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DecrementStock_InsufficientStock_ReturnsConflict()
    {
        var created = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("LowStock", null, 1m, 1));
        var product = await created.Content.ReadFromJsonAsync<ProductResponse>();

        var resp = await _client.PostAsync($"/api/products/{product!.Id}/decrement-stock/100", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Search_ByName_PartialMatch()
    {
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Searchable-Alpha", null, 1m, 1));

        var resp = await _client.GetAsync("/api/products/search?name=alpha");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await resp.Content.ReadFromJsonAsync<List<ProductResponse>>();
        list.Should().Contain(p => p.Name == "Searchable-Alpha");
    }

    [Fact]
    public async Task StockLevel_ReturnsWithinRange()
    {
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("StockRangeTest", null, 1m, 77));

        var resp = await _client.GetAsync("/api/products/stock-level?min=70&max=80");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await resp.Content.ReadFromJsonAsync<List<ProductResponse>>();
        list.Should().Contain(p => p.Name == "StockRangeTest");
    }
}

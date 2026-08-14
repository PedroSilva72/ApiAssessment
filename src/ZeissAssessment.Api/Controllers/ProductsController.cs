using Microsoft.AspNetCore.Mvc;
using ZeissAssessment.Application.Products;

namespace ZeissAssessment.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/decrement-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> DecrementStock(int id, int quantity, CancellationToken cancellationToken) =>
        Ok(await service.DecrementStockAsync(id, quantity, cancellationToken));

    [HttpPost("{id:int}/add-to-stock/{quantity:int}")]
    public async Task<ActionResult<ProductResponse>> AddToStock(int id, int quantity, CancellationToken cancellationToken) =>
        Ok(await service.AddToStockAsync(id, quantity, cancellationToken));

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> Search([FromQuery] string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new ProblemDetails { Title = "Query parameter 'name' is required.", Status = StatusCodes.Status400BadRequest });
        }
        return Ok(await service.SearchByNameAsync(name, cancellationToken));
    }

    [HttpGet("stock-level")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> StockLevel([FromQuery] int min, [FromQuery] int max, CancellationToken cancellationToken) =>
        Ok(await service.GetByStockLevelAsync(min, max, cancellationToken));
}

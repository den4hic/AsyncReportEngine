using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Pagination;
using AsyncReportEngine.Shared.Dtos.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService catalogService;

    public ProductsController(ICatalogService catalogService)
    {
        this.catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var products = await catalogService.GetProductsAsync(page, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await catalogService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto productDto)
    {
        var createdProduct = await catalogService.CreateProductAsync(productDto);
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto productDto)
    {
        var result = await catalogService.UpdateProductAsync(id, productDto);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await catalogService.DeleteProductAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
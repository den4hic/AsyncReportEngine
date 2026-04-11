using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Basic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/lookup")]
public class LookupController : ControllerBase
{
    private readonly ICatalogService catalogService;

    public LookupController(ICatalogService catalogService)
    {
        this.catalogService = catalogService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetCategories()
    {
        var categories = await catalogService.GetCategoriesLookupAsync();
        return Ok(categories);
    }

    [HttpGet("suppliers")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetSuppliers()
    {
        var suppliers = await catalogService.GetSuppliersLookupAsync();
        return Ok(suppliers);
    }
}
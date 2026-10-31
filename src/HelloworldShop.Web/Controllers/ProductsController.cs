using HelloworldShop.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloworldShop.Web.Controllers;

public sealed class ProductsController(ShopDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderByDescending(product => product.IsFeatured)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return View(products);
    }

    [HttpGet("sandals/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Slug == slug, cancellationToken);

        return product is null ? NotFound() : View(product);
    }
}

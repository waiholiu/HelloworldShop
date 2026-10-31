using HelloworldShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloworldShop.Web.Controllers;

public sealed class CartController(ICartService cartService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await cartService.GetAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int productId,
        int quantity = 1,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cartService.AddAsync(productId, quantity, cancellationToken);
            TempData["CartMessage"] = "Added to your bag.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        try
        {
            await cartService.UpdateAsync(productId, quantity, cancellationToken);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }
}

using HelloworldShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelloworldShop.Web.Controllers;

public sealed class CheckoutController(
    ICartService cartService,
    ICheckoutService checkoutService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await cartService.GetAsync(cancellationToken);
        return cart.Items.Count == 0
            ? RedirectToAction("Index", "Cart")
            : View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CancellationToken cancellationToken)
    {
        try
        {
            var order = await checkoutService.PlaceDemoOrderAsync(cancellationToken);
            return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
        }
        catch (InvalidOperationException)
        {
            return RedirectToAction("Index", "Cart");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var order = await checkoutService.GetOrderAsync(orderNumber, cancellationToken);
        return order is null ? NotFound() : View(order);
    }
}

using HelloworldShop.Web.Data;
using HelloworldShop.Web.Models;
using HelloworldShop.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HelloworldShop.Web.Services;

public sealed class CheckoutService(ShopDbContext dbContext, ICartIdentity identity) : ICheckoutService
{
    public async Task<OrderConfirmationViewModel> PlaceDemoOrderAsync(
        CancellationToken cancellationToken = default)
    {
        var browserId = identity.GetOrCreateBrowserId();
        var cart = await dbContext.Carts
            .Include(existing => existing.Items)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(existing => existing.BrowserId == browserId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        var order = new Order
        {
            OrderNumber = $"DEMO-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
            Status = "Demo confirmed",
            Total = cart.Items.Sum(item => item.Product.Price * item.Quantity),
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity
            }).ToList()
        };

        dbContext.Orders.Add(order);
        dbContext.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToConfirmation(order);
    }

    public async Task<OrderConfirmationViewModel?> GetOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(existing => existing.Items)
            .SingleOrDefaultAsync(existing => existing.OrderNumber == orderNumber, cancellationToken);

        return order is null ? null : ToConfirmation(order);
    }

    private static OrderConfirmationViewModel ToConfirmation(Order order) =>
        new(
            order.OrderNumber,
            order.CreatedAt,
            order.Total,
            order.Items.Select(item => new CartLineViewModel(
                item.ProductId,
                item.ProductName,
                string.Empty,
                string.Empty,
                item.UnitPrice,
                item.Quantity)).ToList());
}

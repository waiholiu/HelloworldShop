using HelloworldShop.Web.Data;
using HelloworldShop.Web.Models;
using HelloworldShop.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HelloworldShop.Web.Services;

public sealed class CartService(ShopDbContext dbContext, ICartIdentity identity) : ICartService
{
    public async Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var browserId = identity.GetOrCreateBrowserId();
        var items = await dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.Cart.BrowserId == browserId)
            .OrderBy(item => item.Id)
            .Select(item => new CartLineViewModel(
                item.ProductId,
                item.Product.Name,
                item.Product.Slug,
                item.Product.ImageUrl,
                item.Product.Price,
                item.Quantity))
            .ToListAsync(cancellationToken);

        return new CartViewModel(items);
    }

    public async Task AddAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (!await dbContext.Products.AnyAsync(product => product.Id == productId, cancellationToken))
        {
            throw new KeyNotFoundException("Product not found.");
        }

        var cart = await GetOrCreateCartAsync(cancellationToken);
        var item = await dbContext.CartItems
            .SingleOrDefaultAsync(
                existing => existing.CartId == cart.Id && existing.ProductId == productId,
                cancellationToken);

        if (item is null)
        {
            dbContext.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity = Math.Min(10, item.Quantity + quantity);
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var browserId = identity.GetOrCreateBrowserId();
        var item = await dbContext.CartItems
            .Include(existing => existing.Cart)
            .SingleOrDefaultAsync(
                existing => existing.Cart.BrowserId == browserId && existing.ProductId == productId,
                cancellationToken);

        if (item is null)
        {
            return;
        }

        item.Cart.UpdatedAt = DateTimeOffset.UtcNow;
        if (quantity == 0)
        {
            dbContext.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cart> GetOrCreateCartAsync(CancellationToken cancellationToken)
    {
        var browserId = identity.GetOrCreateBrowserId();
        var cart = await dbContext.Carts
            .SingleOrDefaultAsync(existing => existing.BrowserId == browserId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { BrowserId = browserId };
        dbContext.Carts.Add(cart);
        return cart;
    }
}

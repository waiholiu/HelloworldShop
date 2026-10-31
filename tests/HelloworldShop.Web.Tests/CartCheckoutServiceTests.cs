using HelloworldShop.Web.Data;
using HelloworldShop.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HelloworldShop.Web.Tests;

public sealed class CartCheckoutServiceTests
{
    [Fact]
    public async Task Cart_updates_quantity_and_removes_zero_quantity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        var cart = new CartService(db, new FixedCartIdentity("browser-one"));

        await cart.AddAsync(1, 2);
        await cart.AddAsync(1, 3);
        Assert.Equal(5, (await cart.GetAsync()).ItemCount);

        await cart.UpdateAsync(1, 1);
        Assert.Equal(1, (await cart.GetAsync()).ItemCount);

        await cart.UpdateAsync(1, 0);
        Assert.Empty((await cart.GetAsync()).Items);
    }

    [Fact]
    public async Task Checkout_snapshots_price_and_rejects_empty_cart()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        var identity = new FixedCartIdentity("browser-two");
        var cart = new CartService(db, identity);
        var checkout = new CheckoutService(db, identity);

        await cart.AddAsync(3, 2);
        var confirmation = await checkout.PlaceDemoOrderAsync();

        Assert.Equal(210.00m, confirmation.Total);
        Assert.StartsWith("DEMO-", confirmation.OrderNumber);
        Assert.Empty((await cart.GetAsync()).Items);
        await Assert.ThrowsAsync<InvalidOperationException>(() => checkout.PlaceDemoOrderAsync());
    }

    private static ShopDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ShopDbContext(options);
    }

    private sealed class FixedCartIdentity(string browserId) : ICartIdentity
    {
        public string GetOrCreateBrowserId() => browserId;
    }
}

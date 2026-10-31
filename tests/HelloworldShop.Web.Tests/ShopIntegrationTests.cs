using System.Net;
using System.Text.RegularExpressions;
using HelloworldShop.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelloworldShop.Web.Tests;

public sealed partial class ShopIntegrationTests
{
    [Fact]
    public async Task Catalog_and_details_show_seeded_products()
    {
        await using var factory = new ShopWebApplicationFactory();
        using var client = factory.CreateClient();

        var catalog = await client.GetAsync("/");
        var catalogHtml = await catalog.Content.ReadAsStringAsync();
        var details = await client.GetAsync("/sandals/coastal-drift");
        var detailsHtml = await details.Content.ReadAsStringAsync();
        var missing = await client.GetAsync("/sandals/not-a-product");

        Assert.Equal(HttpStatusCode.OK, catalog.StatusCode);
        Assert.Contains("Good days start from the ground up", catalogHtml);
        Assert.Contains("Coastal Drift", catalogHtml);
        Assert.Contains("Noosa Minimal", catalogHtml);
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        Assert.Contains("softly contoured footbeds", detailsHtml);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Add_to_cart_persists_anonymous_cart_and_items()
    {
        await using var factory = new ShopWebApplicationFactory();
        using var client = factory.CreateClient();

        var productPage = await client.GetStringAsync("/sandals/coastal-drift");
        var token = GetAntiforgeryToken(productPage);
        var response = await client.PostAsync("/Cart/Add", new FormUrlEncodedContent(
        [
            KeyValuePair.Create("__RequestVerificationToken", token),
            KeyValuePair.Create("productId", "1"),
            KeyValuePair.Create("quantity", "2")
        ]));
        var cartHtml = await client.GetStringAsync("/Cart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Coastal Drift", cartHtml);
        Assert.Contains("$178.00", cartHtml);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        Assert.Equal(1, await db.Carts.CountAsync());
        Assert.Equal(2, await db.CartItems.Select(item => item.Quantity).SingleAsync());
    }

    [Fact]
    public async Task Demo_checkout_persists_order_clears_cart_and_never_requests_payment()
    {
        await using var factory = new ShopWebApplicationFactory();
        using var client = factory.CreateClient();

        var productPage = await client.GetStringAsync("/sandals/dune-walker");
        await client.PostAsync("/Cart/Add", new FormUrlEncodedContent(
        [
            KeyValuePair.Create("__RequestVerificationToken", GetAntiforgeryToken(productPage)),
            KeyValuePair.Create("productId", "2"),
            KeyValuePair.Create("quantity", "1")
        ]));

        var checkoutPage = await client.GetStringAsync("/Checkout");
        Assert.Contains("No real payment is collected", checkoutPage);
        Assert.DoesNotContain("card number", checkoutPage, StringComparison.OrdinalIgnoreCase);

        var confirmationResponse = await client.PostAsync(
            "/Checkout/PlaceOrder",
            new FormUrlEncodedContent(
            [
                KeyValuePair.Create("__RequestVerificationToken", GetAntiforgeryToken(checkoutPage))
            ]));
        var confirmationHtml = await confirmationResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, confirmationResponse.StatusCode);
        Assert.Contains("successful simulation", confirmationHtml);
        Assert.Contains("No payment was taken", confirmationHtml);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var order = await db.Orders.Include(item => item.Items).SingleAsync();
        Assert.Equal("Demo confirmed", order.Status);
        Assert.Equal(119.00m, order.Total);
        Assert.Equal("Dune Walker", Assert.Single(order.Items).ProductName);
        Assert.Empty(await db.CartItems.ToListAsync());
    }

    [Fact]
    public async Task Health_endpoint_reports_database_as_healthy()
    {
        await using var factory = new ShopWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("\"status\":\"Healthy\"", body);
        Assert.Contains("\"name\":\"database\"", body);
    }

    private static string GetAntiforgeryToken(string html)
    {
        var match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, "An antiforgery token was not rendered.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}

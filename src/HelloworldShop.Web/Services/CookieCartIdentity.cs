using System.Security.Cryptography;

namespace HelloworldShop.Web.Services;

public sealed class CookieCartIdentity(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment) : ICartIdentity
{
    public const string CookieName = "HelloworldShop.Cart";

    public string GetOrCreateBrowserId()
    {
        var context = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("A request context is required for cart access.");

        if (context.Request.Cookies.TryGetValue(CookieName, out var current)
            && current.Length == 43)
        {
            return current;
        }

        var browserId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        context.Response.Cookies.Append(CookieName, browserId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = environment.IsProduction(),
            MaxAge = TimeSpan.FromDays(30),
            Path = "/"
        });

        return browserId;
    }
}

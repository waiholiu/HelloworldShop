using System.Text.Json;
using HelloworldShop.Web.Data;
using HelloworldShop.Web.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Production requires the DefaultConnection connection string from App Service configuration.");
    }

    builder.Services.AddDbContext<ShopDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
}
else
{
    builder.Services.AddDbContext<ShopDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=helloworldshop.db"));
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ShopDbContext>("database", HealthStatus.Unhealthy, ["ready"]);

if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
    || !string.IsNullOrWhiteSpace(builder.Configuration["ApplicationInsights:ConnectionString"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddScoped<ICartIdentity, CookieCartIdentity>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
            })
        }));
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ShopDbContext>().Database;
    if (database.IsSqlServer())
    {
        await database.MigrateAsync();
    }
    else
    {
        await database.EnsureCreatedAsync();
    }
}

app.Run();

public partial class Program;

using HelloworldShop.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HelloworldShop.Web.Data;

public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.HasData(
                new Product
                {
                    Id = 1,
                    Name = "Coastal Drift",
                    Slug = "coastal-drift",
                    Description = "An easygoing everyday sandal with softly contoured footbeds and adjustable recycled-webbing straps.",
                    Style = "Everyday",
                    Color = "Sea Salt",
                    ImageUrl = "/images/coastal-drift.svg",
                    Price = 89.00m,
                    IsFeatured = true
                },
                new Product
                {
                    Id = 2,
                    Name = "Dune Walker",
                    Slug = "dune-walker",
                    Description = "A trail-ready sandal with secure heel support, grippy tread and quick-drying straps.",
                    Style = "Adventure",
                    Color = "Desert Clay",
                    ImageUrl = "/images/dune-walker.svg",
                    Price = 119.00m,
                    IsFeatured = true
                },
                new Product
                {
                    Id = 3,
                    Name = "Palm Studio",
                    Slug = "palm-studio",
                    Description = "A refined leather-look slide designed for long lunches, gallery days and warm evenings.",
                    Style = "City",
                    Color = "Olive Grove",
                    ImageUrl = "/images/palm-studio.svg",
                    Price = 105.00m,
                    IsFeatured = true
                },
                new Product
                {
                    Id = 4,
                    Name = "Tidal Loop",
                    Slug = "tidal-loop",
                    Description = "A light, water-friendly sandal with a flexible sole and a playful looped upper.",
                    Style = "Water",
                    Color = "Lagoon",
                    ImageUrl = "/images/tidal-loop.svg",
                    Price = 79.00m,
                    IsFeatured = false
                },
                new Product
                {
                    Id = 5,
                    Name = "Sunset Platform",
                    Slug = "sunset-platform",
                    Description = "A cushioned platform sandal that adds height without giving up all-day comfort.",
                    Style = "Statement",
                    Color = "Coral Glow",
                    ImageUrl = "/images/sunset-platform.svg",
                    Price = 129.00m,
                    IsFeatured = false
                },
                new Product
                {
                    Id = 6,
                    Name = "Noosa Minimal",
                    Slug = "noosa-minimal",
                    Description = "Clean lines, a featherweight sole and understated straps for a barely-there feel.",
                    Style = "Minimal",
                    Color = "Midnight",
                    ImageUrl = "/images/noosa-minimal.svg",
                    Price = 95.00m,
                    IsFeatured = false
                });
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(cart => cart.BrowserId).IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();
            entity.HasOne(item => item.Cart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

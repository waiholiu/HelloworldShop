using System.ComponentModel.DataAnnotations;

namespace HelloworldShop.Web.Models;

public sealed class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public required string BrowserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<CartItem> Items { get; set; } = [];
}

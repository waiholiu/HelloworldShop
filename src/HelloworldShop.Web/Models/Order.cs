using System.ComponentModel.DataAnnotations;

namespace HelloworldShop.Web.Models;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(20)]
    public required string OrderNumber { get; set; }

    [MaxLength(32)]
    public required string Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
}

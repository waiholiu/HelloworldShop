using System.ComponentModel.DataAnnotations;

namespace HelloworldShop.Web.Models;

public sealed class OrderItem
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }

    [MaxLength(120)]
    public required string ProductName { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

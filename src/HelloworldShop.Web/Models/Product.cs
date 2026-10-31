using System.ComponentModel.DataAnnotations;

namespace HelloworldShop.Web.Models;

public sealed class Product
{
    public int Id { get; set; }

    [MaxLength(120)]
    public required string Name { get; set; }

    [MaxLength(140)]
    public required string Slug { get; set; }

    [MaxLength(600)]
    public required string Description { get; set; }

    [MaxLength(80)]
    public required string Style { get; set; }

    [MaxLength(80)]
    public required string Color { get; set; }

    [MaxLength(220)]
    public required string ImageUrl { get; set; }

    public decimal Price { get; set; }

    public bool IsFeatured { get; set; }
}

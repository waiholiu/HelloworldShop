namespace HelloworldShop.Web.ViewModels;

public sealed record CartLineViewModel(
    int ProductId,
    string Name,
    string Slug,
    string ImageUrl,
    decimal UnitPrice,
    int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed record CartViewModel(IReadOnlyList<CartLineViewModel> Items)
{
    public int ItemCount => Items.Sum(item => item.Quantity);
    public decimal Total => Items.Sum(item => item.LineTotal);
}

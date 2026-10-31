namespace HelloworldShop.Web.ViewModels;

public sealed record OrderConfirmationViewModel(
    string OrderNumber,
    DateTimeOffset CreatedAt,
    decimal Total,
    IReadOnlyList<CartLineViewModel> Items);

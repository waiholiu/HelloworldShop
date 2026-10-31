using HelloworldShop.Web.ViewModels;

namespace HelloworldShop.Web.Services;

public interface ICheckoutService
{
    Task<OrderConfirmationViewModel> PlaceDemoOrderAsync(
        CancellationToken cancellationToken = default);

    Task<OrderConfirmationViewModel?> GetOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);
}

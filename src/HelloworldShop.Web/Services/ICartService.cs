using HelloworldShop.Web.ViewModels;

namespace HelloworldShop.Web.Services;

public interface ICartService
{
    Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateAsync(int productId, int quantity, CancellationToken cancellationToken = default);
}

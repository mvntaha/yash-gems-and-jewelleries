using Yash_Gems___Jewelleries.ViewModels;

namespace Yash_Gems___Jewelleries.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string? userId, string? guestId);
        Task<bool> AddToCartAsync(string? userId, string? guestId, string styleCode, int quantity);
        Task<bool> UpdateQuantityAsync(string? userId, string? guestId, int cartItemId, string styleCode, int quantity);
        Task<bool> RemoveItemAsync(string? userId, string? guestId, int cartItemId, string styleCode);
        Task ClearCartAsync(string? userId, string? guestId);
        Task MergeGuestCartToUserCartAsync(string userId, string guestId);
        Task<int> GetCartCountAsync(string? userId, string? guestId);
    }
}

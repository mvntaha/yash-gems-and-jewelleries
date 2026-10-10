using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<CartItem> GetCartQuery(string? userId, string? guestId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                return _context.CartItems.Where(c => c.UserId == userId);
            }
            if (!string.IsNullOrEmpty(guestId))
            {
                return _context.CartItems.Where(c => c.GuestId == guestId && c.UserId == null);
            }
            return _context.CartItems.Where(c => false);
        }

        public async Task<CartViewModel> GetCartAsync(string? userId, string? guestId)
        {
            var viewModel = new CartViewModel();
            
            var items = await GetCartQuery(userId, guestId)
                .Include(i => i.Item)
                .ThenInclude(i => i.ProductType)
                .AsNoTracking()
                .Select(item => new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    StyleCode = item.StyleCode,
                    ProductName = item.Item.ItemName,
                    ProductImage = item.Item.PrimaryImageUrl ?? string.Empty,
                    ProductType = item.Item.ProductType != null ? item.Item.ProductType.ProductTypeName : "Standard",
                    Price = item.Item.SellingPrice,
                    Quantity = item.Quantity,
                    MaxStock = item.Item.Quantity
                })
                .ToListAsync();

            foreach (var item in items)
            {
                item.Subtotal = item.Price * item.Quantity;
            }

            viewModel.Items = items;
            viewModel.Subtotal = viewModel.Items.Sum(i => i.Subtotal);
            
            if (viewModel.Items.Any())
            {
                var settings = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
                viewModel.TaxRate = settings?.DefaultTaxRate ?? 3.00m;
                viewModel.Tax = viewModel.Subtotal * (viewModel.TaxRate / 100);
            }

            viewModel.Total = viewModel.Subtotal + viewModel.ShippingCost + viewModel.Tax;

            return viewModel;
        }

        public async Task<bool> AddToCartAsync(string? userId, string? guestId, string styleCode, int quantity)
        {
            var item = await _context.Items.FindAsync(styleCode);
            if (item == null || !item.IsActive) return false;

            var cartItem = await GetCartQuery(userId, guestId)
                .FirstOrDefaultAsync(c => c.StyleCode == styleCode);

            if (cartItem != null)
            {
                int newQty = cartItem.Quantity + quantity;
                if (newQty > item.Quantity) newQty = item.Quantity;
                if (newQty < 1) newQty = 1;

                cartItem.Quantity = newQty;
                cartItem.PriceAtAdd = item.SellingPrice;
                cartItem.CalculateSubtotal();
                cartItem.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                if (quantity < 1) return false;
                int finalQty = Math.Min(quantity, item.Quantity);

                cartItem = new CartItem
                {
                    UserId = userId,
                    GuestId = string.IsNullOrEmpty(userId) ? guestId : null,
                    StyleCode = styleCode,
                    Quantity = finalQty,
                    PriceAtAdd = item.SellingPrice,
                    AddedDate = DateTime.UtcNow
                };
                cartItem.CalculateSubtotal();
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(string? userId, string? guestId, int cartItemId, string styleCode, int quantity)
        {
            var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.StyleCode == styleCode);
            if (item == null || quantity <= 0) return false;

            var cartItem = await GetCartQuery(userId, guestId)
                .FirstOrDefaultAsync(c => (cartItemId > 0 && c.CartItemId == cartItemId) || c.StyleCode == styleCode);

            if (cartItem == null) return false;

            int finalQty = Math.Min(quantity, item.Quantity);
            if (finalQty < 1) finalQty = 1;

            cartItem.Quantity = finalQty;
            cartItem.PriceAtAdd = item.SellingPrice;
            cartItem.CalculateSubtotal();
            cartItem.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemAsync(string? userId, string? guestId, int cartItemId, string styleCode)
        {
            var cartItem = await GetCartQuery(userId, guestId)
                .FirstOrDefaultAsync(c => (cartItemId > 0 && c.CartItemId == cartItemId) || c.StyleCode == styleCode);

            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(string? userId, string? guestId)
        {
            var items = await GetCartQuery(userId, guestId).ToListAsync();
            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MergeGuestCartToUserCartAsync(string userId, string guestId)
        {
            if (string.IsNullOrEmpty(guestId)) return;

            var guestCartItems = await _context.CartItems
                .Where(c => c.GuestId == guestId && c.UserId == null)
                .ToListAsync();

            if (!guestCartItems.Any()) return;

            foreach (var guestItem in guestCartItems)
            {
                var existingUserItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.StyleCode == guestItem.StyleCode);

                if (existingUserItem != null)
                {
                    var product = await _context.Items.FindAsync(guestItem.StyleCode);
                    int maxStock = product?.Quantity ?? guestItem.Quantity;
                    
                    existingUserItem.Quantity = Math.Min(existingUserItem.Quantity + guestItem.Quantity, maxStock);
                    existingUserItem.ModifiedDate = DateTime.UtcNow;
                    _context.CartItems.Remove(guestItem);
                }
                else
                {
                    guestItem.UserId = userId;
                    guestItem.GuestId = null;
                    guestItem.ModifiedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(string? userId, string? guestId)
        {
            // Requirement: reflect the number of unique products (e.g., 1 Item A + 3 Item B = 2)
            return await GetCartQuery(userId, guestId).CountAsync();
        }
    }
}

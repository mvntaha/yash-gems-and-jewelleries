using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ApplicationDbContext _context;

        public LayoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Fetch all active categories with limit
        public async Task<List<Category>> GetCategoriesAsync(int count)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Take(count)
                .ToListAsync();
        }

        // Fetch new arrivals with limit
        public async Task<List<Item>> GetNewArrivalsAsync(int count)
        {
            return await _context.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Brand)
                .Where(i => i.IsActive && i.Quantity > 0)
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        // Fetch brands with limit
        public async Task<List<Brand>> GetBrandsAsync(int count)
        {
            return await _context.Brands
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandType)
                .Take(count)
                .ToListAsync();
        }

        // Fetch featured active products first
        public async Task<List<Item>> GetFeaturedProductsAsync(int count)
        {
            var products = await _context.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Brand)
                .Where(i => i.IsActive && i.IsFeatured && i.Quantity > 0)
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .ToListAsync();

            // Fallback (If not enough featured products, fill with newest active products)
            if (products.Count < count)
            {
                var needed = count - products.Count;
                var existingIds = products.Select(p => p.StyleCode).ToList();
                
                var extra = await _context.Items
                    .AsNoTracking()
                    .Include(i => i.Category)
                    .Include(i => i.Brand)
                    .Where(i => i.IsActive && i.Quantity > 0 && !existingIds.Contains(i.StyleCode))
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(needed)
                    .ToListAsync();
                
                products.AddRange(extra);
            }

            return products;
        }
        public async Task<int> GetWishlistCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return 0;
            return await _context.Wishlists.CountAsync(w => w.UserId == userId);
        }
    }
}

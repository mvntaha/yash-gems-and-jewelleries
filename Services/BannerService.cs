using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Services
{
    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _context;

        public BannerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Banner>> GetHomeBannersAsync()
        {
            var banners = new List<Banner>();

            // Get first active NewIn banner
            var newInBanner = await _context.Banners
                .Include(b => b.DiscountScheme)
                .Where(b => b.IsActive && b.BannerType == BannerType.NewIn)
                .OrderBy(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            if (newInBanner != null) banners.Add(newInBanner);

            // Get first active Promotional banner
            var promotionalBanner = await _context.Banners
                .Include(b => b.DiscountScheme)
                .Where(b => b.IsActive && b.BannerType == BannerType.Promotional)
                .OrderBy(b => b.DisplayOrder)
                .FirstOrDefaultAsync();

            if (promotionalBanner != null) banners.Add(promotionalBanner);

            return banners;
        }

        public async Task<Banner?> GetBannerByIdAsync(int id)
        {
            return await _context.Banners
                .Include(b => b.DiscountScheme)
                .FirstOrDefaultAsync(b => b.BannerId == id);
        }

        public async Task<List<Banner>> GetAllBannersAsync(bool? isActive = null)
        {
            var query = _context.Banners.Include(b => b.DiscountScheme).AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            return await query.OrderBy(b => b.DisplayOrder).ToListAsync();
        }
    }
}

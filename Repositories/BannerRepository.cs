using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Repositories
{
    /// <summary>
    /// Repository implementation for Banner entity with optimized queries
    /// </summary>
    public class BannerRepository : MasterDataRepository<Banner>, IBannerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BannerRepository> _logger;

        public BannerRepository(
            ApplicationDbContext context,
            ILogger<BannerRepository> logger) : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Banner>> GetActiveBannersByTypeAsync(string bannerType)
        {
            try
            {
                if (!Enum.TryParse<BannerType>(bannerType, out var parsedBannerType))
                {
                    _logger.LogWarning("Invalid BannerType value: {BannerType}", bannerType);
                    return Enumerable.Empty<Banner>();
                }

                var banners = await _context.Banners
                    .AsNoTracking()
                    .Where(b => b.IsActive && b.BannerType == parsedBannerType)
                    .OrderBy(b => b.DisplayOrder)
                    .ThenByDescending(b => b.CreatedDate)
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} active banners of type {BannerType}",
                    banners.Count,
                    bannerType);

                return banners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active banners of type {BannerType}", bannerType);
                throw;
            }
        }

    }
}

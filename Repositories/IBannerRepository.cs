using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Repositories
{
    /// <summary>
    /// Repository interface for Banner entity operations
    /// </summary>
    public interface IBannerRepository : IMasterDataRepository<Banner>
    {
        /// <summary>
        /// Get active banners by type, ordered by DisplayOrder
        /// </summary>
        Task<IEnumerable<Banner>> GetActiveBannersByTypeAsync(string bannerType);
    }
}

using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface IBannerService
    {
        Task<List<Banner>> GetHomeBannersAsync();
        Task<Banner?> GetBannerByIdAsync(int id);
        Task<List<Banner>> GetAllBannersAsync(bool? isActive = null);
    }
}

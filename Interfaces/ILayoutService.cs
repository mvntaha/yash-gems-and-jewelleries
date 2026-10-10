using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface ILayoutService
    {
        Task<List<Category>> GetCategoriesAsync(int count);
        Task<List<Item>> GetFeaturedProductsAsync(int count);
        Task<List<Item>> GetNewArrivalsAsync(int count);
        Task<List<Brand>> GetBrandsAsync(int count);
        Task<int> GetWishlistCountAsync(string userId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync(bool? isActive = null);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<bool> CreateBrandAsync(Brand brand);
        Task<bool> UpdateBrandAsync(Brand brand);
        Task<bool> DeleteBrandAsync(int id);
        Task<bool> BrandExistsAsync(int id);
        Task<bool> BrandNameExistsAsync(string name, int? excludeId = null);
    }
}

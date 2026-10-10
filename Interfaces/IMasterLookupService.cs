using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IMasterLookupService
    {
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<ProductType>> GetActiveProductTypesAsync();
        Task<IEnumerable<GoldKarat>> GetActiveGoldKaratsAsync();
        Task<IEnumerable<Certificate>> GetActiveCertificatesAsync();
        Task<IEnumerable<DiamondQuality>> GetActiveDiamondQualitiesAsync();
        Task<IEnumerable<StoneQuality>> GetActiveStoneQualitiesAsync();

        Task<bool> IsBrandValidAsync(int id);
        Task<bool> IsCategoryValidAsync(int id);
        Task<bool> IsProductTypeValidAsync(int id);
        Task<bool> IsGoldKaratValidAsync(int id);
        Task<bool> IsCertificateValidAsync(int? id);
        Task<bool> IsDiamondQualityValidAsync(int id);
        Task<bool> IsStoneQualityValidAsync(int id);
    }
}

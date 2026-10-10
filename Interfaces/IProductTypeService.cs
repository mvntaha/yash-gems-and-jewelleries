using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IProductTypeService
    {
        Task<IEnumerable<ProductType>> GetAllProductTypesAsync();
        Task<ProductType?> GetProductTypeByIdAsync(int id);
        Task<bool> CreateProductTypeAsync(ProductType productType);
        Task<bool> UpdateProductTypeAsync(ProductType productType);
        Task<bool> DeleteProductTypeAsync(int id);
        Task<bool> ProductTypeNameExistsAsync(string name, int? excludeId = null);
    }
}

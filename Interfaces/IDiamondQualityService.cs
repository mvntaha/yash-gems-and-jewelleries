using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IDiamondQualityService
    {
        Task<IEnumerable<DiamondQuality>> GetAllDiamondQualitiesAsync();
        Task<DiamondQuality?> GetDiamondQualityByIdAsync(int id);
        Task<bool> CreateDiamondQualityAsync(DiamondQuality quality);
        Task<bool> UpdateDiamondQualityAsync(DiamondQuality quality);
        Task<bool> DeleteDiamondQualityAsync(int id);
        Task<bool> QualityGradeExistsAsync(string grade, int? excludeId = null);
    }
}

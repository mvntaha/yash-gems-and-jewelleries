using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IStoneQualityService
    {
        Task<IEnumerable<StoneQuality>> GetAllStoneQualitiesAsync();
        Task<StoneQuality?> GetStoneQualityByIdAsync(int id);
        Task<bool> CreateStoneQualityAsync(StoneQuality quality);
        Task<bool> UpdateStoneQualityAsync(StoneQuality quality);
        Task<bool> DeleteStoneQualityAsync(int id);
        Task<bool> StoneTypeExistsAsync(string type, int? excludeId = null);
    }
}

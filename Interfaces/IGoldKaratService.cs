using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IGoldKaratService
    {
        Task<IEnumerable<GoldKarat>> GetAllGoldKaratsAsync();
        Task<GoldKarat?> GetGoldKaratByIdAsync(int id);
        Task<bool> CreateGoldKaratAsync(GoldKarat karat);
        Task<bool> UpdateGoldKaratAsync(GoldKarat karat);
        Task<bool> DeleteGoldKaratAsync(int id);
        Task<bool> GoldCaratExistsAsync(string carat, int? excludeId = null);
    }
}

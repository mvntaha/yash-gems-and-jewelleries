using System.Collections.Generic;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Services
{
    public interface ICertificateService
    {
        Task<IEnumerable<Certificate>> GetAllCertificatesAsync();
        Task<Certificate?> GetCertificateByIdAsync(int id);
        Task<bool> CreateCertificateAsync(Certificate certificate);
        Task<bool> UpdateCertificateAsync(Certificate certificate);
        Task<bool> DeleteCertificateAsync(int id);
        Task<bool> CertificateTypeExistsAsync(string type, int? excludeId = null);
    }
}

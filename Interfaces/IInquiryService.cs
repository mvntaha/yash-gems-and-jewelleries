using Yash_Gems___Jewelleries.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IInquiryService
    {
        Task<IEnumerable<Inquiry>> GetAllInquiriesAsync(int page = 1, int pageSize = 10, string? search = null, bool? isActive = null);
        Task<int> GetInquiriesCountAsync(string? search = null, bool? isActive = null);
        Task<Inquiry?> GetInquiryByIdAsync(int id);
        Task<bool> SubmitReplyAsync(int inquiryId, string adminResponse, string respondedBy, string? status = null);
        Task<bool> UpdateStatusAsync(int inquiryId, string status);
        Task<bool> ToggleStatusAsync(int inquiryId);
        Task<bool> CreateInquiryAsync(Inquiry inquiry);
    }
}

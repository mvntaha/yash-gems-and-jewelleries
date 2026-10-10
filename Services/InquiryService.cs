using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Services
{
    public class InquiryService : IInquiryService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public InquiryService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Inquiry>> GetAllInquiriesAsync(int page = 1, int pageSize = 10, string? search = null, bool? isActive = null)
        {
            var query = GetFilteredInquiryQuery(search, isActive);

            return await query
                .Include(i => i.User)
                .Include(i => i.Item)
                .OrderByDescending(i => i.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetInquiriesCountAsync(string? search = null, bool? isActive = null)
        {
            return await GetFilteredInquiryQuery(search, isActive).CountAsync();
        }

        private IQueryable<Inquiry> GetFilteredInquiryQuery(string? search, bool? isActive)
        {
            var query = _context.Inquiries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => 
                    i.FullName.Contains(search) || 
                    i.Email.Contains(search) || 
                    i.Subject.Contains(search) || 
                    (i.PhoneNumber != null && i.PhoneNumber.Contains(search)));
            }

            if (isActive.HasValue)
            {
                if (isActive.Value)
                {
                    query = query.Where(i => i.Status != "Closed" && i.Status != "Deactivated");
                }
                else
                {
                    query = query.Where(i => i.Status == "Closed" || i.Status == "Deactivated");
                }
            }

            return query;
        }

        public async Task<Inquiry?> GetInquiryByIdAsync(int id)
        {
            return await _context.Inquiries
                .Include(i => i.User)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.InquiryId == id);
        }

        public async Task<bool> SubmitReplyAsync(int inquiryId, string adminResponse, string respondedBy, string? status = null)
        {
            try
            {
                var inquiry = await _context.Inquiries.FindAsync(inquiryId);
                if (inquiry == null) return false;

                inquiry.AdminResponse = adminResponse;
                inquiry.RespondedBy = respondedBy;
                inquiry.ResponseDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(status))
                {
                    inquiry.Status = status;
                }
                else
                {
                    inquiry.Status = "Resolved"; // Default to Resolved if no status provided
                }
                
                _context.Inquiries.Update(inquiry);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ToggleStatusAsync(int inquiryId)
        {
            try
            {
                var inquiry = await _context.Inquiries.FindAsync(inquiryId);
                if (inquiry == null) return false;
                
                if (inquiry.Status == "Closed")
                {
                    inquiry.Status = "InProgress"; // Re-open
                }
                else
                {
                    inquiry.Status = "Closed"; // Close/Deactivate
                }

                inquiry.ModifiedDate = DateTime.UtcNow;
                _context.Inquiries.Update(inquiry);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreateInquiryAsync(Inquiry inquiry)
        {
            try
            {
                inquiry.CreatedDate = DateTime.UtcNow;
                inquiry.Status = "New";
                _context.Inquiries.Add(inquiry);
                await _context.SaveChangesAsync();

                // Trigger Notification
                await _notificationService.CreateNotificationAsync(
                    "New Inquiry Submission",
                    $"New inquiry from {inquiry.FullName} about {inquiry.Subject}",
                    NotificationType.InquirySubmission,
                    inquiry.InquiryId.ToString(),
                    inquiry.FullName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UpdateStatusAsync(int inquiryId, string status)
        {
            try
            {
                var inquiry = await _context.Inquiries.FindAsync(inquiryId);
                if (inquiry == null) return false;

                inquiry.Status = status;
                inquiry.ModifiedDate = DateTime.UtcNow;

                _context.Inquiries.Update(inquiry);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

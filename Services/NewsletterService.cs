using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly ApplicationDbContext _context;

        public NewsletterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SubscribeAsync(string email, string? fullName = null, string? ipAddress = null, string? source = null)
        {
            try
            {
                email = email.Trim().ToLower();
                var existing = await _context.NewsletterSubscriptions
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == email);

                if (existing != null)
                {
                    if (existing.IsSubscribed)
                    {
                        return true; // Already subscribed, treat as success but don't add duplicate
                    }
                    
                    // Re-subscribe if previously unsubscribed
                    existing.IsSubscribed = true;
                    existing.FullName = fullName ?? existing.FullName;
                    existing.IpAddress = ipAddress ?? existing.IpAddress;
                    existing.Source = source ?? existing.Source;
                    existing.SubscribedDate = DateTime.UtcNow;
                    existing.UnsubscribedDate = null;
                    
                    _context.NewsletterSubscriptions.Update(existing);
                }
                else
                {
                    var subscription = new NewsletterSubscription
                    {
                        Email = email,
                        FullName = fullName,
                        IpAddress = ipAddress,
                        Source = source,
                        IsSubscribed = true,
                        SubscribedDate = DateTime.UtcNow
                    };
                    _context.NewsletterSubscriptions.Add(subscription);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> IsEmailSubscribedAsync(string email)
        {
            return await _context.NewsletterSubscriptions
                .AnyAsync(s => s.Email.ToLower() == email.Trim().ToLower() && s.IsSubscribed);
        }

        public async Task<IEnumerable<NewsletterSubscription>> GetAllSubscriptionsAsync()
        {
            return await _context.NewsletterSubscriptions
                .OrderByDescending(s => s.SubscribedDate)
                .ToListAsync();
        }
    }
}

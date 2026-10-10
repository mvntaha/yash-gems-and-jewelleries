using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface INewsletterService
    {
        Task<bool> SubscribeAsync(string email, string? fullName = null, string? ipAddress = null, string? source = null);
        Task<bool> IsEmailSubscribedAsync(string email);
        Task<IEnumerable<NewsletterSubscription>> GetAllSubscriptionsAsync();
    }
}

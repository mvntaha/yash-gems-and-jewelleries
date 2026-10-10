using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string title, string message, NotificationType type, string? referenceId = null, string? createdBy = null, string targetRole = "Admin");
        Task<List<Notification>> GetLatestNotificationsAsync(int count = 5);
        Task<int> GetUnreadCountAsync();
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> BulkMarkAsReadAsync();
        Task<(List<Notification> Items, int TotalCount)> GetPagedNotificationsAsync(int pageIndex, int pageSize, bool? isRead = null, NotificationType? type = null);
        Task<bool> DeleteNotificationAsync(int id);
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Hubs;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Yash_Gems___Jewelleries.Hubs.NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context, 
            ILogger<NotificationService> logger,
            Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(string title, string message, NotificationType type, string? referenceId = null, string? createdBy = null, string targetRole = "Admin")
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    ReferenceId = referenceId,
                    CreatedBy = createdBy,
                    TargetRole = targetRole,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // SignalR push
                await _hubContext.Clients.All.SendAsync("UpdateNotificationCount", await GetUnreadCountAsync());
                await _hubContext.Clients.All.SendAsync("ReceiveNotificationRefresh");
            }
            catch (Exception ex)
            {
                // Graceful failure: Log but don't rethrow to avoid breaking core flows
                _logger.LogError(ex, "Error creating notification: {Title}", title);
            }
        }

        public async Task<List<Notification>> GetLatestNotificationsAsync(int count = 5)
        {
            return await _context.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkMarkAsReadAsync()
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any()) return true;

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetPagedNotificationsAsync(int pageIndex, int pageSize, bool? isRead = null, NotificationType? type = null)
        {
            var query = _context.Notifications.AsNoTracking().AsQueryable();

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(n => n.Type == type.Value);
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

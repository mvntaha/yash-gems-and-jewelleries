using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10, bool? isRead = null, NotificationType? type = null)
        {
            var (items, totalCount) = await _notificationService.GetPagedNotificationsAsync(pageIndex, pageSize, isRead, type);
            
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.IsRead = isRead;
            ViewBag.Type = type;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var notifications = await _notificationService.GetLatestNotificationsAsync();
            var unreadCount = await _notificationService.GetUnreadCountAsync();
            return Json(new { notifications, unreadCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = result, message = result ? "Notification marked as read." : "Failed to update notification.", type = result ? "success" : "error" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.BulkMarkAsReadAsync();
            return Json(new { success = result, message = result ? "All notifications marked as read." : "Failed to update notifications.", type = result ? "success" : "error" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Json(new { success = result, message = result ? "Notification deleted." : "Failed to delete notification.", type = result ? "success" : "error" });
        }
    }
}

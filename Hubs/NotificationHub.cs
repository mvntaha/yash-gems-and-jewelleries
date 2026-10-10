using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Yash_Gems___Jewelleries.Hubs
{
    [Authorize(Roles = "Admin")]
    public class NotificationHub : Hub
    {
        public async Task SendNotificationCount(int count)
        {
            await Clients.All.SendAsync("UpdateNotificationCount", count);
        }

        public async Task RefreshNotifications()
        {
            await Clients.All.SendAsync("ReceiveNotificationRefresh");
        }
    }
}

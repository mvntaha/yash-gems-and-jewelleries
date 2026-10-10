using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IOrderEmailService
    {
        /// <summary>
        /// Sends a confirmation email to the customer after a successful order
        /// </summary>
        Task SendOrderConfirmationEmailAsync(Order order);

        /// <summary>
        /// Sends a notification email to the admin/vendor when a new order is placed
        /// </summary>
        Task SendNewOrderAdminNotificationAsync(Order order);
    }
}

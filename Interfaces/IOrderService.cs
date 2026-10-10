using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IOrderService
    {
        Task<Order?> PlaceOrderAsync(OrderPlacementViewModel model, string userId);
        Task<Order?> GetOrderByIdAsync(int orderId, string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId, int pageIndex = 1, int pageSize = 10);
        Task<int> GetUserOrdersCountAsync(string userId);
        Task<Order?> GetOrderForAdminAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<List<Order>> GetAllOrdersAsync(string status, int pageIndex = 1, int pageSize = 10);
        Task<int> GetAllOrdersCountAsync(string status);
        Task<OrderStatsViewModel> GetOrderStatsAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? trackingNumber = null, string? courierService = null, string? notes = null, string? changedBy = null);
        Task<bool> CancelOrderAsync(int orderId, string adminNotes);
        
        // Timeline
        List<OrderTimelineStep> GetOrderTimeline(Order order);
    }
}

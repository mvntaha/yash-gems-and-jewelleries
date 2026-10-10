using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: Order/Index (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string status = "All", int pageIndex = 1, int pageSize = 10)
        {
            var orders = await _orderService.GetAllOrdersAsync(status, pageIndex, pageSize);
            var totalCount = await _orderService.GetAllOrdersCountAsync(status);
            var stats = await _orderService.GetOrderStatsAsync();

            var viewModel = new AdminOrderIndexViewModel
            {
                Orders = orders,
                Stats = stats,
                CurrentStatus = status,
                Pagination = new PaginationMetadata
                {
                    TotalItems = totalCount,
                    PageSize = pageSize,
                    CurrentPage = pageIndex
                }
            };

            return View(viewModel);
        }

        // GET: Order/Detail (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            Order? order = null;

            // Try to resolve as numeric OrderId first
            if (int.TryParse(id, out int orderId))
            {
                order = await _orderService.GetOrderForAdminAsync(orderId);
            }

            // If not found by ID (or ID was alphanumeric), try resolving as OrderNumber
            if (order == null)
            {
                order = await _orderService.GetOrderByNumberAsync(id);
            }

            if (order == null) return NotFound();

            // Generate Timeline
            var timeline = _orderService.GetOrderTimeline(order);

            var viewModel = new OrderDetailViewModel
            {
                Order = order,
                Timeline = timeline
            };

            return View(viewModel);
        }

        // POST: Order/UpdateOrderStatus (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus orderStatus, string trackingNumber, string courierService)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, orderStatus, trackingNumber, courierService, changedBy: "Admin");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (success)
                    return Json(new { success = true, message = "Order status updated successfully.", type = "success" });
                return Json(new { success = false, message = "Order not found or update failed.", type = "danger" });
            }

            if (!success)
            {
                TempData["Error"] = "Order not found or update failed. Please ensure you are following a valid status transition (e.g. Pending -> Confirmed).";
                return RedirectToAction(nameof(Index), "Order", new { area = "Admin" });
            }

            TempData["Success"] = "Order status updated successfully.";
            return RedirectToAction(nameof(Detail), "Order", new { area = "Admin", id = orderId });
        }

        // POST: Order/CancelOrder (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId, string adminNotes)
        {
            var success = await _orderService.CancelOrderAsync(orderId, adminNotes);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (success)
                    return Json(new { success = true, message = "Order cancelled successfully. Stock has been restored.", type = "success" });
                return Json(new { success = false, message = "Order not found or cancellation failed.", type = "danger" });
            }

            if (!success)
            {
                TempData["Error"] = "Order not found or cancellation failed.";
                return RedirectToAction(nameof(Index), "Order", new { area = "Admin" });
            }

            TempData["Success"] = "Order cancelled successfully. Stock has been restored.";
            return RedirectToAction(nameof(Detail), "Order", new { area = "Admin", id = orderId });
        }
    }
}
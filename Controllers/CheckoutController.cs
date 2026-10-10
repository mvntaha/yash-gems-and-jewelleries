using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Services;

namespace Yash_Gems___Jewelleries.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IOrderService orderService, 
            ICartService cartService, 
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var guestId = Request.Cookies["GuestCartId"];
            var cart = await _cartService.GetCartAsync(user.Id, guestId);
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new OrderPlacementViewModel
            {
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                EmailOrPhone = user.Email ?? user.PhoneNumber ?? "",
                Phone = user.PhoneNumber ?? "",
                Address = user.Address ?? "",
                City = user.City ?? "",
                State = user.State ?? "",
                ZipCode = user.PostalCode ?? "",
                Country = user.State ?? "",
                Cart = cart
            };

            return View(model);
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(OrderPlacementViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Re-load cart for summary display if validation fails
            var guestId = Request.Cookies["GuestCartId"];
            model.Cart = await _cartService.GetCartAsync(user.Id, guestId);

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var order = await _orderService.PlaceOrderAsync(model, user.Id);
                if (order != null)
                {
                    return RedirectToAction("Success", new { id = order.OrderId });
                }
                
                ModelState.AddModelError("", "Your cart is empty or an error occurred.");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"{ex.Message}");
                return View("Index", model);
            }
        }

        // GET: Checkout/Success
        public async Task<IActionResult> Success(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _orderService.GetOrderByIdAsync(id, user.Id);
            if (order == null) return NotFound();

            return View(order);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly Data.ApplicationDbContext _context;
        private const string GuestCartCookieName = "GuestCartId";

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager, ICompositeViewEngine viewEngine, Data.ApplicationDbContext context)
        {
            _cartService = cartService;
            _userManager = userManager;
            _viewEngine = viewEngine;
            _context = context;
        }

        private string? GetUserId() => _userManager.GetUserId(User);

        private string GetGuestId()
        {
            var guestId = Request.Cookies[GuestCartCookieName];
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append(GuestCartCookieName, guestId, cookieOptions);
            }
            return guestId;
        }

        private async Task<string> RenderPartialViewToString(string viewName, object? model)
        {
            ViewData.Model = model;
            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
            if (viewResult.View == null) viewResult = _viewEngine.GetView(null, viewName, false);

            if (viewResult.View == null) throw new ArgumentNullException($"{viewName} not found");

            var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }

        public async Task<IActionResult> Index()
        {
            var cartViewModel = await _cartService.GetCartAsync(GetUserId(), GetGuestId());
            
            cartViewModel.RecommendedProducts = await _context.Items
                .Where(i => i.IsActive && i.Quantity > 0)
                .OrderBy(r => Guid.NewGuid())
                .Take(8)
                .ToListAsync();

            var userId = GetUserId();
            if (userId != null)
            {
                cartViewModel.WishlistStyleCodes = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.StyleCode)
                    .ToListAsync();
            }

            return View(cartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string styleCode, int quantity = 1)
        {
            var userId = GetUserId();
            var guestId = GetGuestId();
            var success = await _cartService.AddToCartAsync(userId, guestId, styleCode, quantity);
            
            if (!success) return Json(new { success = false, message = "Could not add item (Out of stock or invalid)." });

            var cartViewModel = await _cartService.GetCartAsync(userId, guestId);
            var cartCount = await _cartService.GetCartCountAsync(userId, guestId);
            
            return Json(new { 
                success = true, 
                message = "Item added to cart.", 
                cartCount = cartCount,
                sidebarHtml = await RenderPartialViewToString("_CartSidebarPartial", cartViewModel),
                indicatorHtml = await RenderPartialViewToString("_CartIndicatorPartial", cartCount)
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, string styleCode, int quantity)
        {
            var userId = GetUserId();
            var guestId = GetGuestId();
            var success = await _cartService.UpdateQuantityAsync(userId, guestId, cartItemId, styleCode, quantity);

            if (!success) return Json(new { success = false, message = "Could not update quantity." });

            var cartViewModel = await _cartService.GetCartAsync(userId, guestId);
            var cartCount = await _cartService.GetCartCountAsync(userId, guestId);

            return Json(new { 
                success = true, 
                message = "Cart updated.",
                cartCount = cartCount,
                sidebarHtml = await RenderPartialViewToString("_CartSidebarPartial", cartViewModel),
                itemsHtml = await RenderPartialViewToString("_CartItemsPartial", cartViewModel),
                summaryHtml = await RenderPartialViewToString("_CartSummaryPartial", cartViewModel),
                indicatorHtml = await RenderPartialViewToString("_CartIndicatorPartial", cartCount)
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId, string styleCode)
        {
            var userId = GetUserId();
            var guestId = GetGuestId();
            var success = await _cartService.RemoveItemAsync(userId, guestId, cartItemId, styleCode);
            
            if (!success) return Json(new { success = false, message = "Item not found." });

            var cartViewModel = await _cartService.GetCartAsync(userId, guestId);
            var cartCount = await _cartService.GetCartCountAsync(userId, guestId);

            return Json(new { 
                success = true, 
                message = "Item removed.",
                cartCount = cartCount,
                sidebarHtml = await RenderPartialViewToString("_CartSidebarPartial", cartViewModel),
                itemsHtml = await RenderPartialViewToString("_CartItemsPartial", cartViewModel),
                summaryHtml = await RenderPartialViewToString("_CartSummaryPartial", cartViewModel),
                indicatorHtml = await RenderPartialViewToString("_CartIndicatorPartial", cartCount)
            });
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var guestId = GetGuestId();
            await _cartService.ClearCartAsync(userId, guestId);
            
            var cartViewModel = await _cartService.GetCartAsync(userId, guestId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    cartCount = 0,
                    sidebarHtml = await RenderPartialViewToString("_CartSidebarPartial", cartViewModel),
                    itemsHtml = await RenderPartialViewToString("_CartItemsPartial", cartViewModel),
                    summaryHtml = await RenderPartialViewToString("_CartSummaryPartial", cartViewModel),
                    indicatorHtml = await RenderPartialViewToString("_CartIndicatorPartial", 0)
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCartInfo()
        {
            var count = await _cartService.GetCartCountAsync(GetUserId(), GetGuestId());
            return Json(new { success = true, count = count });
        }
    }
}

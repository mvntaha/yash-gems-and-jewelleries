using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBannerService _bannerService;
        private readonly IDiscountService _discountService;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            IBannerService bannerService,
            IDiscountService discountService)
        {
            _logger = logger;
            _context = context;
            _bannerService = bannerService;
            _discountService = discountService;
        }

        // GET: Home
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Exquisite Handcrafted Jewelry";
            ViewData["MetaDescription"] = "Discover the finest collection of handcrafted jewelry at Yash Gems & Jewelleries. From elegant rings to stunning necklaces, find your signature look.";
            
            var viewModel = new HomeViewModel();

            // 1. Fetch Banners (optimized via Service)
            var homeBanners = await _bannerService.GetHomeBannersAsync();

            var firstBanner = homeBanners.FirstOrDefault(b => b.BannerType == BannerType.NewIn);
            if (firstBanner != null)
            {
                viewModel.FirstBanner = new BannerHomeViewModel
                {
                    Title = firstBanner.Title,
                    Description = firstBanner.Description,
                    ImageUrl = firstBanner.ImageUrl,
                    LinkUrl = firstBanner.LinkUrl,
                    ButtonText = firstBanner.ButtonText,
                    BannerType = firstBanner.BannerType.ToString()
                };
            }

            var secondBanner = homeBanners.FirstOrDefault(b => b.BannerType == BannerType.Promotional);
            if (secondBanner != null)
            {
                viewModel.SecondBanner = new BannerHomeViewModel
                {
                    Title = secondBanner.Title,
                    Description = secondBanner.Description,
                    ImageUrl = secondBanner.ImageUrl,
                    LinkUrl = secondBanner.LinkUrl,
                    ButtonText = secondBanner.ButtonText,
                    BannerType = secondBanner.BannerType.ToString()
                };

                // Apply discount messaging only if a valid scheme exists
                if (secondBanner.DiscountScheme != null && _discountService.IsSchemeValid(secondBanner.DiscountScheme))
                {
                    viewModel.SecondBanner.DiscountMessage = _discountService.GetDiscountMessage(secondBanner.DiscountScheme);
                }
            }

            // 2. Fetch Top 5 Best Products (Deterministic rule: highest sold quantity via OrderItems)
            viewModel.BestProducts = await _context.Items
                .AsNoTracking()
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.OrderItems.Sum(oi => (int?)oi.Quantity) ?? 0)
                .Take(5)
                .Select(i => new ProductHomeViewModel
                {
                    StyleCode = i.StyleCode,
                    ItemName = i.ItemName,
                    MRP = i.MRP,
                    SellingPrice = i.SellingPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    PrimaryImageUrl = i.PrimaryImageUrl,
                    SecondaryImageUrl = i.SecondaryImageUrl,
                    IsNew = i.IsNewLaunch,
                    IsOnSale = i.IsOnSale,
                    Pair = i.Pairs,
                    CategoryName = i.Category.CategoryName,
                    BrandName = i.Brand.BrandType
                })
                .ToListAsync();

            // 3. Newly Launched Single Product
            viewModel.NewLaunchProduct = await _context.Items
                .AsNoTracking()
                .Where(i => i.IsActive && i.IsNewLaunch)
                .OrderByDescending(i => i.CreatedDate)
                .Select(i => new ProductHomeViewModel
                {
                    StyleCode = i.StyleCode,
                    ItemName = i.ItemName,
                    MRP = i.MRP,
                    SellingPrice = i.SellingPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    PrimaryImageUrl = i.PrimaryImageUrl,
                    SecondaryImageUrl = i.SecondaryImageUrl,
                    IsNew = i.IsNewLaunch,
                    IsOnSale = i.IsOnSale,
                    Pair = i.Pairs,
                    CategoryName = i.Category.CategoryName,
                    BrandName = i.Brand.BrandType
                })
                .FirstOrDefaultAsync();

            // 4. Featured Collections (Active categories with top displayed number)
            viewModel.FeaturedCollections = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Take(5) // Matching the 5 items in the template
                .Select(c => new CategoryHomeViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    ItemCount = c.Items.Count(i => i.IsActive)
                })
                .ToListAsync();

            // 5. Fetch Active Discount Schemes (Active, within date range and usage limit)
            var now = DateTime.UtcNow;
            viewModel.ActiveDiscountSchemes = await _context.DiscountSchemes
                .AsNoTracking()
                .Where(ds => ds.IsActive && now >= ds.StartDate && now <= ds.EndDate)
                .Where(ds => ds.UsageLimit == null || ds.UsageCount < ds.UsageLimit)
                .OrderByDescending(ds => ds.IsFeatured)
                .ThenByDescending(ds => ds.CreatedDate)
                .Select(ds => new DiscountSchemeHomeViewModel
                {
                    DiscountSchemeId = ds.DiscountSchemeId,
                    SchemeName = ds.SchemeName,
                    Description = ds.Description,
                    DiscountType = ds.DiscountType,
                    DiscountValue = ds.DiscountValue,
                    CouponCode = ds.CouponCode,
                    EndDate = ds.EndDate
                })
                .ToListAsync();

            // 6. Wishlist Style Codes (if authenticated)
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    viewModel.WishlistStyleCodes = await _context.Wishlists
                        .AsNoTracking()
                        .Where(w => w.UserId == userId)
                        .Select(w => w.StyleCode)
                        .ToListAsync();
                }
            }

            return View(viewModel);
        }

        // GET: Home/Learn
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult Learn()
        {
            return View();
        }

        // GET: Home/AboutUs
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult AboutUs()
        {
            return View();
        }

        // GET: Home/ContactUs
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult ContactUs()
        {
            return View();
        }

        // POST: Home/ContactUs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(Inquiry inquiry)
        {
            if (ModelState.IsValid)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var recentInquiry = await _context.Inquiries
                    .Where(i => i.IpAddress == ipAddress && i.CreatedDate > DateTime.UtcNow.AddMinutes(-5))
                    .AnyAsync();

                if (recentInquiry)
                {
                    ModelState.AddModelError("", "You have recently submitted an inquiry. Please wait a few minutes before submitting another one.");
                    return View(inquiry);
                }

                // Set default values
                inquiry.CreatedDate = DateTime.UtcNow;
                inquiry.Status = "New";
                inquiry.IpAddress = ipAddress;
                inquiry.Priority = 2; // Medium

                // Get user ID if authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    inquiry.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                }

                _context.Inquiries.Add(inquiry);
                await _context.SaveChangesAsync();

                TempData["SuccessInquiry"] = "Your inquiry has been submitted successfully. We will get back to you soon.";
                return RedirectToAction(nameof(ContactUs));
            }

            return View(inquiry);
        }

        // POST: Home/SubscribeNewsletter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeNewsletter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(Index));
            }

            // Check if email already exists
            var existing = await _context.NewsletterSubscriptions
                .FirstOrDefaultAsync(n => n.Email == email);

            if (existing != null)
            {
                if (existing.IsSubscribed)
                {
                    TempData["Info"] = "This email is already subscribed to our newsletter.";
                }
                else
                {
                    existing.IsSubscribed = true;
                    existing.SubscribedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "You have been re-subscribed to our newsletter!";
                }
            }
            else
            {
                var subscription = new NewsletterSubscription
                {
                    Email = email,
                    SubscribedDate = DateTime.UtcNow,
                    IsSubscribed = true
                };

                _context.NewsletterSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you for subscribing to our newsletter!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

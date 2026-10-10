using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Repositories;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDashboardService _dashboardService;

        private readonly IBannerService _bannerService;
        private readonly IDiscountService _discountService;
        private readonly IImageService _imageService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            IDashboardService dashboardService,
            IBannerService bannerService,
            IDiscountService discountService,
            IImageService imageService,
            ILogger<DashboardController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dashboardService = dashboardService;
            _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _dashboardService.GetDashboardSummaryAsync();
                return View(viewModel);
            }
            catch (Exception)
            {
                // Log error
                // Return view with empty model or error message to prevent crash
                return View(new ViewModels.DashboardViewModel());
            }
        }

        [HttpGet]
        [Route("Admin/Dashboard/GetSalesChartData")]
        public async Task<IActionResult> GetSalesChartData(string period = "1Y")
        {
            try
            {
                var data = await _dashboardService.GetSalesChartDataAsync(period);
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error loading chart data", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("Admin/Dashboard/GetConversionData")]
        public async Task<IActionResult> GetConversionData()
        {
            try
            {
                var data = await _dashboardService.GetOrderConversionDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error loading conversion data", error = ex.Message });
            }
        }

        /// <summary>
        /// Banner management index page
        /// </summary>
        [HttpGet]
        [Route("Admin/Dashboard/Banners")]
        public async Task<IActionResult> Banners(bool? isActive = null)
        {
            try
            {
                var banners = await _bannerService.GetAllBannersAsync(isActive);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Banners/_BannerTablePartial", banners);
                }

                ViewBag.IsActive = isActive;
                return View("Banners/Index", banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banners list");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
                });
            }
        }

        /// <summary>
        /// Get Banner modal for Add/Edit
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBannerModal(int? id)
        {
            try
            {
                BannerViewModel model;

                if (id.HasValue && id.Value > 0)
                {
                    var banner = await _bannerService.GetBannerByIdAsync(id.Value);
                    if (banner == null) return NotFound();

                    model = new BannerViewModel
                    {
                        BannerId = banner.BannerId,
                        Title = banner.Title,
                        Description = banner.Description,
                        ButtonText = banner.ButtonText,
                        ButtonLink = banner.LinkUrl,
                        DisplayOrder = banner.DisplayOrder,
                        BannerType = banner.BannerType,
                        StartDate = banner.StartDate,
                        EndDate = banner.EndDate,
                        IsActive = banner.IsActive,
                        DiscountSchemeId = banner.DiscountSchemeId,
                        ExistingImageUrl = banner.ImageUrl,
                        ExistingMobileImageUrl = banner.MobileImageUrl,
                        RowVersion = banner.RowVersion
                    };
                }
                else
                {
                    // Creation is not allowed per requirements
                    return BadRequest("Creating new banners is not allowed.");
                }

                var activeSchemes = await _discountService.GetActiveSchemesAsync();
                model.DiscountSchemes = activeSchemes.Select(s => new SelectListItem
                {
                    Value = s.DiscountSchemeId.ToString(),
                    Text = $"{s.SchemeName} ({_discountService.GetDiscountMessage(s)})"
                }).ToList();
 
                return PartialView("Banners/_BannerFormPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner modal for ID {Id}", id);
                return BadRequest("An error occurred while loading the form.");
            }
        }

        /// <summary>
        /// Save Banner (Create or Update)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBanner(BannerViewModel model)
        {
            if (model.BannerId <= 0)
            {
                return BadRequest("Creating new banners is not allowed.");
            }

            if (!ModelState.IsValid)
            {
                var activeSchemes = await _discountService.GetActiveSchemesAsync();
                model.DiscountSchemes = activeSchemes.Select(s => new SelectListItem
                {
                    Value = s.DiscountSchemeId.ToString(),
                    Text = $"{s.SchemeName} ({_discountService.GetDiscountMessage(s)})"
                }).ToList();
                return PartialView("Banners/_BannerFormPartial", model);
            }
 
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string? imageUrl = model.ExistingImageUrl;
                string? mobileImageUrl = model.ExistingMobileImageUrl;
 
                // Handle image uploads
                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(model.ExistingImageUrl)) _imageService.DeleteImage(model.ExistingImageUrl);
                    imageUrl = await _imageService.SaveImageAsync(model.ImageFile, IImageService.Banners);
                }
 
                if (model.MobileImageFile != null)
                {
                    if (!string.IsNullOrEmpty(model.ExistingMobileImageUrl)) _imageService.DeleteImage(model.ExistingMobileImageUrl);
                    mobileImageUrl = await _imageService.SaveImageAsync(model.MobileImageFile, IImageService.Banners);
                }
 
                // Update existing banner
                var existing = await _context.Banners.FindAsync(model.BannerId);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.ButtonText = model.ButtonText;
                existing.LinkUrl = model.ButtonLink;
                existing.DisplayOrder = model.DisplayOrder;
                existing.BannerType = model.BannerType;
                existing.StartDate = model.StartDate;
                existing.EndDate = model.EndDate;
                existing.IsActive = model.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = userId;

                // Handle DiscountSchemeId restriction
                if (model.BannerType == BannerType.Promotional)
                {
                    existing.DiscountSchemeId = model.DiscountSchemeId;
                }
                else
                {
                    existing.DiscountSchemeId = null;
                }

                existing.ImageUrl = imageUrl ?? string.Empty;
                existing.MobileImageUrl = mobileImageUrl;

                _context.Banners.Update(existing);
                await _context.SaveChangesAsync();
 
                return Json(new { success = true, message = "Banner saved successfully!", type = "success" });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "The banner was modified by another user. Please refresh and try again.");
                var activeSchemes = await _discountService.GetActiveSchemesAsync();
                model.DiscountSchemes = activeSchemes.Select(s => new SelectListItem
                {
                    Value = s.DiscountSchemeId.ToString(),
                    Text = $"{s.SchemeName} ({_discountService.GetDiscountMessage(s)})"
                }).ToList();
                return PartialView("Banners/_BannerFormPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving banner");
                ModelState.AddModelError("", "An error occurred while saving the banner.");
                var activeSchemes = await _discountService.GetActiveSchemesAsync();
                model.DiscountSchemes = activeSchemes.Select(s => new SelectListItem
                {
                    Value = s.DiscountSchemeId.ToString(),
                    Text = $"{s.SchemeName} ({_discountService.GetDiscountMessage(s)})"
                }).ToList();
                return PartialView("Banners/_BannerFormPartial", model);
            }
        }

        /// <summary>
        /// Toggle Banner status
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBannerStatus(int id)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null)
                {
                    return Json(new { success = false, message = "Banner not found" });
                }
 
                banner.IsActive = !banner.IsActive;
                banner.ModifiedDate = DateTime.UtcNow;
                banner.ModifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
                _context.Banners.Update(banner);
                await _context.SaveChangesAsync();
 
                return Json(new { success = true, message = $"Status updated to {(banner.IsActive ? "Active" : "Inactive")}.", type = "success", isActive = banner.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner status for ID {Id}", id);
                return Json(new { success = false, message = "An error occurred while updating status." });
            }
        }

        /// <summary>
        /// Delete Banner
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            return Json(new { success = false, message = "Deleting banners is not allowed." });
        }

        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            var viewModel = new SettingsViewModel();
            try
            {
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();

                if (settings != null)
                {
                    viewModel.OwnerPhoneNumber = settings.OwnerPhoneNumber;
                    viewModel.OwnerEmail = settings.OwnerEmail;
                    viewModel.FullAddress = settings.FullAddress;
                    viewModel.ZipCode = settings.ZipCode;
                    viewModel.City = settings.City;
                    viewModel.Country = settings.Country;
                    viewModel.DefaultTaxRate = settings.DefaultTaxRate;

                    // Readonly fields for display
                    ViewBag.PricesWithTax = settings.PricesWithTax;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings from database");
            }

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setting(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Please fix the validation errors.", type = "danger" });
                }

                // Repopulate readonly fields for View if validation fails
                var currentSettings = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
                if (currentSettings != null)
                {
                    ViewBag.PricesWithTax = currentSettings.PricesWithTax;
                    ViewBag.DefaultTaxRate = currentSettings.DefaultTaxRate;
                }

                return View(model);
            }

            try
            {
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();

                if (settings == null)
                {
                    settings = new SystemSetting
                    {
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.SystemSettings.Add(settings);
                }

                // Strictly update only these fields
                settings.OwnerPhoneNumber = model.OwnerPhoneNumber;
                settings.OwnerEmail = model.OwnerEmail;
                settings.FullAddress = model.FullAddress;
                settings.ZipCode = model.ZipCode;
                settings.City = model.City;
                settings.Country = model.Country;
                settings.DefaultTaxRate = model.DefaultTaxRate;
                settings.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Settings updated successfully!", type = "success" });
                }

                TempData["SuccessMessage"] = "Settings updated successfully!";
                return RedirectToAction(nameof(Setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while saving settings.", type = "danger" });
                }

                ModelState.AddModelError("", "An error occurred while saving settings.");
                return View(model);
            }
        }

    }
}

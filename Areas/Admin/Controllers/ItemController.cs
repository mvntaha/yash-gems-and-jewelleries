using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.ViewModels;
using System.Linq;
using System.Security.Claims;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IMasterLookupService _masterLookup;
        private readonly IImageService _imageService;
        private readonly ILogger<ItemController> _logger;

        public ItemController(
            IItemService itemService,
            IMasterLookupService masterLookup,
            IImageService imageService,
            ILogger<ItemController> logger)
        {
            _itemService = itemService;
            _masterLookup = masterLookup;
            _imageService = imageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? brandId, int? categoryId, bool? isActive, int page = 1)
        {
            try
            {
                int pageSize = 10;
                var items = await _itemService.GetAllProductsAsync(searchTerm, brandId, categoryId, isActive, page, pageSize);
                var totalCount = await _itemService.GetTotalCountAsync(searchTerm, brandId, categoryId, isActive);

                // Populate ViewBag only for full page load
                if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") 
                {
                    ViewBag.Brands = await _masterLookup.GetActiveBrandsAsync();
                    ViewBag.Categories = await _masterLookup.GetActiveCategoriesAsync();
                }

                ViewBag.SearchTerm = searchTerm;
                ViewBag.BrandId = brandId;
                ViewBag.CategoryId = categoryId;
                ViewBag.IsActive = isActive;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ItemTablePartial", items);
                }

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product list");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _masterLookup.GetActiveBrandsAsync();
            return Json(brands.Select(b => new { id = b.BrandId, name = b.BrandType }));
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _masterLookup.GetActiveCategoriesAsync();
            return Json(categories.Select(c => new { id = c.CategoryId, name = c.CategoryName }));
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await PopulateViewBags();
            
            // Generate a temporary unique StyleCode (e.g., YASH-2026-XXXX)
            string year = DateTime.Now.Year.ToString();
            string random = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string styleCode = $"YASH-{year}-{random}";
            
            return View(new ItemCreateViewModel { StyleCode = styleCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState invalid for CreateProduct: {Errors}", string.Join(", ", errors));
                
                // Diagnostic logging
                _logger.LogWarning("Diagnostic - Received Model: ItemName='{Name}', StyleCode='{Style}', GoldWeight={Weight}", 
                    model.ItemName, model.StyleCode, model.GoldWeight);

                try 
                {
                    var formContent = string.Join(", ", Request.Form.Select(f => $"{f.Key}='{f.Value}'"));
                    _logger.LogWarning("Diagnostic - Raw Form Data: {Form}", formContent);
                }
                catch { }

                await PopulateViewBags();
                return View(model);
            }

            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _itemService.CreateProductAsync(model, userId);

                _logger.LogInformation("CreateProductAsync result: {Result} for StyleCode: {StyleCode}", result, model.StyleCode);

                if (result)
                {
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create product. Please check the logs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {StyleCode}", model.StyleCode);
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
            }

            await PopulateViewBags();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var item = await _itemService.GetProductByStyleCodeAsync(id, includeDetails: true);
            if (item == null) return NotFound();

            var model = new ItemEditViewModel
            {
                StyleCode = item.StyleCode,
                ItemName = item.ItemName,
                Description = item.Description,
                Pairs = item.Pairs,
                Quantity = item.Quantity,
                BrandId = item.BrandId,
                CategoryId = item.CategoryId,
                CertificateId = item.CertificateId,
                ProductTypeId = item.ProductTypeId,
                GoldTypeId = item.GoldTypeId,
                GoldWeight = item.GoldWeight,
                StoneWeight = item.StoneWeight,
                WastagePercentage = item.WastagePercentage,
                GoldRate = item.GoldRate,
                GoldMakingCharges = item.GoldMakingCharges,
                StoneMakingCharges = item.StoneMakingCharges,
                OtherMakingCharges = item.OtherMakingCharges,
                DiscountPercentage = item.DiscountPercentage,
                IsActive = item.IsActive,
                IsFeatured = item.IsFeatured,
                IsNewLaunch = item.IsNewLaunch,
                IsOnSale = item.IsOnSale,
                MetaTitle = item.MetaTitle,
                MetaDescription = item.MetaDescription,
                MetaKeywords = item.MetaKeywords,
                ExistingPrimaryImageUrl = item.PrimaryImageUrl,
                ExistingSecondaryImageUrl = item.SecondaryImageUrl,
                ExistingTertiaryImageUrl = item.TertiaryImageUrl,

                DiamondDetails = item.DiamondDetails.Select(d => new DiamondDetailViewModel
                {
                    DiamondDetailId = d.DiamondDetailId,
                    DiamondQualityId = d.DiamondQualityId,
                    Carat = d.Carat,
                    Pieces = d.Pieces,
                    Weight = d.Weight,
                    Rate = d.Rate,
                    Shape = d.Shape,
                    SettingType = d.SettingType,
                    Remarks = d.Remarks
                }).ToList(),

                StoneDetails = item.StoneDetails.Select(s => new StoneDetailViewModel
                {
                    StoneDetailId = s.StoneDetailId,
                    StoneQualityId = s.StoneQualityId,
                    Weight = s.Weight,
                    Pieces = s.Pieces,
                    Carat = s.Carat,
                    Rate = s.Rate,
                    Shape = s.Shape,
                    SettingType = s.SettingType,
                    Treatment = s.Treatment,
                    Remarks = s.Remarks
                }).ToList()
            };

            await PopulateViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState invalid for UpdateProduct: {Errors}", string.Join(", ", errors));

                 // Diagnostic logging
                _logger.LogWarning("Diagnostic - Received Model: ItemName='{Name}', StyleCode='{Style}', GoldWeight={Weight}", 
                    model.ItemName, model.StyleCode, model.GoldWeight);

                try 
                {
                    var formContent = string.Join(", ", Request.Form.Select(f => $"{f.Key}='{f.Value}'"));
                    _logger.LogWarning("Diagnostic - Raw Form Data: {Form}", formContent);
                }
                catch { }

                await PopulateViewBags();
                return View(model);
            }

            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _itemService.UpdateProductAsync(model, userId);

                _logger.LogInformation("UpdateProductAsync result: {Result} for StyleCode: {StyleCode}", result, model.StyleCode);

                if (result)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update product. Please check the logs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {StyleCode}", model.StyleCode);
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
            }

            await PopulateViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _itemService.DeleteProductAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Product deleted successfully.", type = "success" });
                }
                return Json(new { success = false, message = "Failed to delete product.", type = "danger" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {StyleCode}", id);
                return Json(new { success = false, message = "An error occurred.", type = "danger" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var result = await _itemService.ToggleStatusAsync(id);
                return Json(new { success = true, message = $"Status updated to {(result ? "Active" : "Inactive")}.", type = "success", success_toggle = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for product: {StyleCode}", id);
                return Json(new { success = false, message = "Failed to update status.", type = "danger" });
            }
        }

        private async Task PopulateViewBags()
        {
            var brands = await _masterLookup.GetActiveBrandsAsync();
            ViewBag.Brands = brands;
            ViewBag.BrandList = new SelectList(brands, "BrandId", "BrandType");

            var categories = await _masterLookup.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.CategoryList = new SelectList(categories, "CategoryId", "CategoryName");

            var productTypes = await _masterLookup.GetActiveProductTypesAsync();
            ViewBag.ProductTypes = productTypes;
            ViewBag.ProductTypeList = new SelectList(productTypes, "ProductTypeId", "ProductTypeName");

            var goldKarats = await _masterLookup.GetActiveGoldKaratsAsync();
            ViewBag.GoldKarats = goldKarats;
            ViewBag.GoldKaratList = new SelectList(goldKarats, "GoldTypeId", "GoldCarat");

            var certificates = await _masterLookup.GetActiveCertificatesAsync();
            ViewBag.Certificates = certificates;
            ViewBag.CertificateList = new SelectList(certificates, "CertificateId", "CertifyType");

            var diamondQualities = await _masterLookup.GetActiveDiamondQualitiesAsync();
            ViewBag.DiamondQualities = diamondQualities;
            ViewBag.DiamondQualityList = new SelectList(diamondQualities, "DiamondQualityId", "QualityGrade");

            var stoneQualities = await _masterLookup.GetActiveStoneQualitiesAsync();
            ViewBag.StoneQualities = stoneQualities;
            ViewBag.StoneQualityList = stoneQualities.Select(q => new SelectListItem 
            { 
                Value = q.StoneQualityId.ToString(), 
                Text = $"{q.StoneType} - {q.QualityGrade ?? "Standard"}"
            }).ToList();
        }
    }
}
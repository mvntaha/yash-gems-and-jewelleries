using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Models.Enums;
using Yash_Gems___Jewelleries.Data;

namespace Yash_Gems___Jewelleries.Controllers
{
    public class ItemController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ItemController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Item/Index
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> Index(
            int[]? brandIds,
            int[]? categoryIds,
            int[]? goldTypeIds,
            string[]? stoneColors,
            int[]? productTypeIds,
            string[]? diamondClarities,
            string[]? diamondColors,
            string[]? diamondCuts,
            string? caratRange,
            decimal? minPrice,
            decimal? maxPrice,
            string? availability,
            string? sortBy,
            string? searchQuery,
            int page = 1)
        {
            int pageSize = 12;

            var viewModel = new ItemFilterViewModel
            {
                BrandIds = brandIds,
                CategoryIds = categoryIds,
                GoldTypeIds = goldTypeIds,
                StoneColors = stoneColors,
                ProductTypeIds = productTypeIds,
                DiamondClarities = diamondClarities,
                DiamondColors = diamondColors,
                DiamondCuts = diamondCuts,
                CaratRange = caratRange,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Availability = availability,
                SortBy = sortBy,
                SearchQuery = searchQuery,
                Page = page,
                PageSize = pageSize
            };

            var query = _context.Items
                .Include(i => i.Brand)
                .Include(i => i.Category)
                .Include(i => i.ProductType)
                .Include(i => i.GoldKarat)
                .Include(i => i.StoneDetails).ThenInclude(s => s.StoneQuality)
                .Include(i => i.DiamondDetails).ThenInclude(d => d.DiamondQuality)
                .Where(i => i.IsActive)
                .AsQueryable();

            // Apply Search Filter First
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                query = query.Where(i => 
                    i.ItemName.Contains(q) || 
                    i.StyleCode.Contains(q) || 
                    i.Category.CategoryName.Contains(q) || 
                    i.Brand.BrandType.Contains(q) ||
                    i.GoldKarat.GoldCarat.Contains(q)
                );
            }

            // Convert arrays to Lists to avoid LINQ translation issues with primitive arrays (ReadOnlySpan conflict)
            var selectedBrandIds = brandIds?.ToList() ?? new List<int>();
            var selectedCategoryIds = categoryIds?.ToList() ?? new List<int>();
            var selectedGoldTypeIds = goldTypeIds?.ToList() ?? new List<int>();
            var selectedProductTypeIds = productTypeIds?.ToList() ?? new List<int>();
            var selectedStoneColors = stoneColors?.ToList() ?? new List<string>();
            var selectedDiamondClarities = diamondClarities?.ToList() ?? new List<string>();
            var selectedDiamondColors = diamondColors?.ToList() ?? new List<string>();
            var selectedDiamondCuts = diamondCuts?.ToList() ?? new List<string>();

            // Apply filters
            if (selectedBrandIds.Any())
                query = query.Where(i => selectedBrandIds.Contains(i.BrandId));

            if (selectedCategoryIds.Any())
                query = query.Where(i => selectedCategoryIds.Contains(i.CategoryId));

            if (selectedGoldTypeIds.Any())
                query = query.Where(i => selectedGoldTypeIds.Contains(i.GoldTypeId));

            if (selectedProductTypeIds.Any())
                query = query.Where(i => selectedProductTypeIds.Contains(i.ProductTypeId));

            if (selectedStoneColors.Any())
                query = query.Where(i => i.StoneDetails.Any(sd => sd.StoneQuality != null && selectedStoneColors.Contains(sd.StoneQuality.Color)));

            if (selectedDiamondClarities.Any())
                query = query.Where(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && selectedDiamondClarities.Contains(dd.DiamondQuality.Clarity)));

            if (selectedDiamondColors.Any())
                query = query.Where(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && selectedDiamondColors.Contains(dd.DiamondQuality.Color)));

            if (selectedDiamondCuts.Any())
                query = query.Where(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && selectedDiamondCuts.Contains(dd.DiamondQuality.Cut)));

            if (!string.IsNullOrEmpty(caratRange))
            {
                var parts = caratRange.Split('-');
                if (parts.Length == 2 && decimal.TryParse(parts[0], out decimal minCarat) && decimal.TryParse(parts[1], out decimal maxCarat))
                {
                    query = query.Where(i => i.DiamondDetails.Any(dd => dd.Carat >= minCarat && dd.Carat <= maxCarat));
                }
                else if (caratRange.StartsWith("1.0+"))
                {
                     query = query.Where(i => i.DiamondDetails.Any(dd => dd.Carat >= 1.0m));
                }
            }

            if (minPrice.HasValue)
                query = query.Where(i => i.SellingPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(i => i.SellingPrice <= maxPrice.Value);

            if (availability == "inStock")
                query = query.Where(i => i.Quantity > 0);
            else if (availability == "outStock")
                query = query.Where(i => i.Quantity <= 0);

            // Apply sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(i => i.SellingPrice),
                "price_desc" => query.OrderByDescending(i => i.SellingPrice),
                "name_asc" => query.OrderBy(i => i.ItemName),
                "name_desc" => query.OrderByDescending(i => i.ItemName),
                "newest" => query.OrderByDescending(i => i.CreatedDate),
                _ => query.OrderByDescending(i => i.CreatedDate)
            };

            viewModel.TotalItems = await query.CountAsync();
            viewModel.Items = await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var baseQuery = _context.Items.AsNoTracking().Where(i => i.IsActive);
            
            viewModel.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new ItemFilterOption {
                    Id = c.CategoryId,
                    Label = c.CategoryName,
                    ImageUrl = c.ImageUrl,
                    Count = baseQuery.Count(i => i.CategoryId == c.CategoryId)
                }).ToListAsync();

            foreach (var opt in viewModel.Categories)
                opt.IsSelected = selectedCategoryIds.Contains(opt.Id);

            viewModel.Brands = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new ItemFilterOption {
                    Id = b.BrandId,
                    Label = b.BrandType,
                    ImageUrl = b.LogoUrl,
                    Count = baseQuery.Count(i => i.BrandId == b.BrandId)
                }).ToListAsync();

            foreach (var opt in viewModel.Brands)
                opt.IsSelected = selectedBrandIds.Contains(opt.Id);

            viewModel.ProductTypes = await _context.ProductTypes
                .Where(p => p.IsActive)
                .Select(p => new ItemFilterOption {
                    Id = p.ProductTypeId,
                    Label = p.ProductTypeName,
                    ImageUrl = p.ImageUrl,
                    Count = baseQuery.Count(i => i.ProductTypeId == p.ProductTypeId)
                }).ToListAsync();

            foreach (var opt in viewModel.ProductTypes)
                opt.IsSelected = selectedProductTypeIds.Contains(opt.Id);

            viewModel.Materials = await _context.GoldKarats
                .Where(g => g.IsActive)
                .Select(g => new ItemFilterOption {
                    Id = g.GoldTypeId,
                    Label = g.GoldCarat,
                    Count = baseQuery.Count(i => i.GoldTypeId == g.GoldTypeId)
                }).ToListAsync();

            foreach (var opt in viewModel.Materials)
                opt.IsSelected = selectedGoldTypeIds.Contains(opt.Id);

            // Stone Color counts
            var stoneColorsData = await _context.StoneQualities
                .Where(sq => sq.IsActive && !string.IsNullOrEmpty(sq.Color))
                .Select(sq => sq.Color)
                .Distinct()
                .ToListAsync();

            foreach (var color in stoneColorsData)
            {
                if (string.IsNullOrEmpty(color)) continue;
                viewModel.StoneColorsList.Add(new ItemFilterOption {
                    Value = color,
                    Label = color,
                    Count = baseQuery.Count(i => i.StoneDetails.Any(sd => sd.StoneQuality != null && sd.StoneQuality.Color == color)),
                    IsSelected = selectedStoneColors.Contains(color)
                });
            }

            // Diamond filters
            var diamondQualities = await _context.DiamondQualities.Where(d => d.IsActive).ToListAsync();

            // Diamond Clarity
            var validClarities = diamondQualities.Where(d => !string.IsNullOrEmpty(d.Clarity)).Select(d => d.Clarity).Distinct().ToList();
            foreach (var clarity in validClarities)
            {
                 if (string.IsNullOrEmpty(clarity)) continue;
                 viewModel.DiamondClaritiesList.Add(new ItemFilterOption {
                    Value = clarity,
                    Label = clarity,
                    Count = baseQuery.Count(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && dd.DiamondQuality.Clarity == clarity)),
                    IsSelected = selectedDiamondClarities.Contains(clarity)
                 });
            }

            // Diamond Color
            var validColors = diamondQualities.Where(d => !string.IsNullOrEmpty(d.Color)).Select(d => d.Color).Distinct().ToList();
            foreach (var color in validColors)
            {
                 if (string.IsNullOrEmpty(color)) continue;
                 viewModel.DiamondColorsList.Add(new ItemFilterOption {
                    Value = color,
                    Label = color,
                    Count = baseQuery.Count(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && dd.DiamondQuality.Color == color)),
                    IsSelected = selectedDiamondColors.Contains(color)
                 });
            }

            // Diamond Cut
            var validCuts = diamondQualities.Where(d => !string.IsNullOrEmpty(d.Cut)).Select(d => d.Cut).Distinct().ToList();
            foreach (var cut in validCuts)
            {
                 if (string.IsNullOrEmpty(cut)) continue;
                 viewModel.DiamondCutsList.Add(new ItemFilterOption {
                    Value = cut,
                    Label = cut,
                    Count = baseQuery.Count(i => i.DiamondDetails.Any(dd => dd.DiamondQuality != null && dd.DiamondQuality.Cut == cut)),
                    IsSelected = selectedDiamondCuts.Contains(cut)
                 });
            }

            viewModel.InStockCount = await baseQuery.CountAsync(i => i.Quantity > 0);
            viewModel.OutOfStockCount = await baseQuery.CountAsync(i => i.Quantity <= 0);

            // Wishlist
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                viewModel.WishlistStyleCodes = await _context.Wishlists
                    .Where(w => w.UserId == user.Id)
                    .Select(w => w.StyleCode)
                    .ToListAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductListPartial", viewModel);
            }

            return View(viewModel);
        }

        // GET: Item/ProductDetail/{styleCode}
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false, VaryByQueryKeys = new[] { "id" })]
        public async Task<IActionResult> ProductDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var item = await _context.Items
                .AsNoTracking()
                .Include(i => i.Brand)
                .Include(i => i.Category)
                .Include(i => i.Certificate)
                .Include(i => i.ProductType)
                .Include(i => i.GoldKarat)
                .Include(i => i.DiamondDetails).ThenInclude(dd => dd.DiamondQuality)
                .Include(i => i.StoneDetails).ThenInclude(sd => sd.StoneQuality)
                .FirstOrDefaultAsync(i => i.StyleCode == id && i.IsActive);

            if (item == null)
            {
                return NotFound();
            }

            // SEO Tags
            ViewBag.Description = item.MetaDescription ?? item.Description ?? item.ItemName;
            ViewData["Title"] = item.MetaTitle ?? item.ItemName;
            ViewData["MetaDescription"] = item.MetaDescription ?? (item.Description?.Length > 160 ? item.Description.Substring(0, 157) + "..." : item.Description);
            ViewData["MetaKeywords"] = item.MetaKeywords ?? $"{item.ItemName}, {item.Category?.CategoryName}, {item.Brand?.BrandType}, SC{item.StyleCode}";
            ViewData["OgType"] = "product";
            ViewData["OgImage"] = item.PrimaryImageUrl;

            // Fetch Related Products (same category, active, excluding current)
            var relatedProducts = await _context.Items
                .AsNoTracking()
                .Where(i => i.CategoryId == item.CategoryId && i.StyleCode != item.StyleCode && i.IsActive)
                .Take(8)
                .ToListAsync();

            // Recently Viewed logic
            var recentlyViewed = Request.Cookies["RecentlyViewed"];
            var viewedCodes = new List<string>();
            if (!string.IsNullOrEmpty(recentlyViewed))
            {
                viewedCodes = recentlyViewed.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // Update Cookie: Remove current if exists, and add to front
            viewedCodes.Remove(item.StyleCode);
            viewedCodes.Insert(0, item.StyleCode);
            var updatedCodes = viewedCodes.Take(10).ToList();
            Response.Cookies.Append("RecentlyViewed", string.Join(",", updatedCodes), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });

            // Fetch recently viewed items (excluding current)
            var otherViewedCodes = updatedCodes.Where(c => c != item.StyleCode).ToList();
            var recentlyViewedItemsList = await _context.Items
                .AsNoTracking()
                .Where(i => otherViewedCodes.Contains(i.StyleCode) && i.IsActive)
                .ToListAsync();
            
            // Order them according to the cookie list
            var sortedRecentlyViewed = otherViewedCodes
                .Select(code => recentlyViewedItemsList.FirstOrDefault(i => i.StyleCode == code))
                .Where(i => i != null)
                .Cast<Item>()
                .ToList();

            // Fetch user wishlist
            var user = await _userManager.GetUserAsync(User);
            var wishlistStyleCodes = new List<string>();
            if (user != null)
            {
                wishlistStyleCodes = await _context.Wishlists
                    .AsNoTracking()
                    .Where(w => w.UserId == user.Id)
                    .Select(w => w.StyleCode)
                    .ToListAsync();
            }

            var viewModel = new ProductDetailViewModel
            {
                Item = item,
                RelatedProducts = relatedProducts,
                RecentlyViewedItems = sortedRecentlyViewed,
                WishlistStyleCodes = wishlistStyleCodes
            };

            return View(viewModel);
        }

        // GET: Item/GetQuickDetails/{id}
        [HttpGet]
        public async Task<IActionResult> GetQuickDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid product ID");
            }

            var item = await _context.Items
                .Include(i => i.Brand)
                .Include(i => i.Category)
                .Include(i => i.ProductType)
                .Include(i => i.GoldKarat)
                .Include(i => i.DiamondDetails).ThenInclude(d => d.DiamondQuality)
                .Include(i => i.StoneDetails).ThenInclude(s => s.StoneQuality)
                .FirstOrDefaultAsync(i => i.StyleCode == id && i.IsActive);

            if (item == null)
            {
                return NotFound("Product not found");
            }

            // Check if wishlisted
            var user = await _userManager.GetUserAsync(User);
            bool isWishlisted = false;
            if (user != null)
            {
                isWishlisted = await _context.Wishlists.AnyAsync(w => w.UserId == user.Id && w.StyleCode == item.StyleCode);
            }

            // Map to a clean anonymous object to prevent over-fetching and circular references
            var result = new
            {
                styleCode = item.StyleCode,
                itemName = item.ItemName,
                description = item.Description,
                sellingPrice = item.SellingPrice,
                sellingPriceFormatted = item.SellingPrice.ToString("C"),
                mrp = item.MRP,
                mrpFormatted = item.MRP.ToString("C"),
                discountPercentage = item.DiscountPercentage,
                isOnSale = item.IsOnSale || item.DiscountPercentage > 0,
                quantity = item.Quantity,
                availability = item.Quantity > 0 ? "In stock" : "Out of stock",
                brand = item.Brand?.BrandType,
                category = item.Category?.CategoryName,
                productType = item.ProductType?.ProductTypeName,
                goldKarat = item.GoldKarat?.GoldCarat,
                images = new[] { item.PrimaryImageUrl, item.SecondaryImageUrl, item.TertiaryImageUrl }
                            .Where(img => !string.IsNullOrEmpty(img))
                            .ToList(),
                diamondQuality = item.DiamondDetails.FirstOrDefault()?.DiamondQuality?.QualityGrade,
                stoneQuality = item.StoneDetails.FirstOrDefault()?.StoneQuality?.QualityGrade,
                isWishlisted = isWishlisted
            };

            return Json(result);
        }
        // GET: Item/QuickView/{id}
        [HttpGet]
        public async Task<IActionResult> QuickView(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid product ID");
            }

            var item = await _context.Items
                .Include(i => i.Brand)
                .Include(i => i.Category)
                .Include(i => i.ProductType)
                .Include(i => i.GoldKarat)
                .Include(i => i.DiamondDetails).ThenInclude(d => d.DiamondQuality)
                .Include(i => i.StoneDetails).ThenInclude(s => s.StoneQuality)
                .FirstOrDefaultAsync(i => i.StyleCode == id && i.IsActive);

            if (item == null)
            {
                return NotFound("Product not found");
            }

            // Check if wishlisted
            var user = await _userManager.GetUserAsync(User);
            bool isWishlisted = false;
            if (user != null)
            {
                isWishlisted = await _context.Wishlists.AnyAsync(w => w.UserId == user.Id && w.StyleCode == item.StyleCode);
            }

            ViewBag.IsWishlisted = isWishlisted;

            return PartialView("_QuickViewPartial", item);
        }
    }
}



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Enums;
using Yash_Gems___Jewelleries.ViewModels;

namespace Yash_Gems___Jewelleries.Services
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IMasterLookupService _masterLookup;
        private readonly ILogger<ItemService> _logger;
        private readonly INotificationService _notificationService;

        public ItemService(ApplicationDbContext context, IImageService imageService, IMasterLookupService masterLookup, ILogger<ItemService> logger, INotificationService notificationService)
        {
            _context = context;
            _imageService = imageService;
            _masterLookup = masterLookup;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Item>> GetAllProductsAsync(string? searchTerm, int? brandId, int? categoryId, bool? isActive, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Fetching products with filters - SearchTerm: {SearchTerm}, BrandId: {BrandId}, CategoryId: {CategoryId}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
                    searchTerm, brandId, categoryId, isActive, page, pageSize);

                var query = GetFilteredQuery(searchTerm, brandId, categoryId, isActive);

                var items = await query
                    .Include(i => i.Brand)
                    .Include(i => i.Category)
                    .Include(i => i.ProductType)
                    .Include(i => i.GoldKarat)
                    .Include(i => i.OrderItems)
                    .OrderByDescending(i => i.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Successfully fetched {Count} products for page {Page}", items.Count, page);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products with filters - SearchTerm: {SearchTerm}, BrandId: {BrandId}, CategoryId: {CategoryId}, IsActive: {IsActive}, Page: {Page}",
                    searchTerm, brandId, categoryId, isActive, page);
                throw;
            }
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm, int? brandId, int? categoryId, bool? isActive)
        {
            return await GetFilteredQuery(searchTerm, brandId, categoryId, isActive).CountAsync();
        }

        public async Task<Item?> GetProductByStyleCodeAsync(string styleCode, bool includeDetails = false)
        {
            var query = _context.Items.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(i => i.Brand)
                    .Include(i => i.Category)
                    .Include(i => i.Certificate)
                    .Include(i => i.ProductType)
                    .Include(i => i.GoldKarat)
                    .Include(i => i.DiamondDetails).ThenInclude(d => d.DiamondQuality)
                    .Include(i => i.StoneDetails).ThenInclude(s => s.StoneQuality);
            }

            return await query.FirstOrDefaultAsync(i => i.StyleCode == styleCode);
        }

        public async Task<bool> CreateProductAsync(ItemCreateViewModel model, string? userId)
        {
            // Validate StyleCode uniqueness
            if (await StyleCodeExistsAsync(model.StyleCode))
            {
                _logger.LogWarning("CreateProduct failed: StyleCode {StyleCode} already exists.", model.StyleCode);
                throw new InvalidOperationException($"Style Code '{model.StyleCode}' already exists. Please use a unique Code.");
            }

            // Validate Master Data
            if (!await _masterLookup.IsBrandValidAsync(model.BrandId)) throw new ArgumentException("Invalid BrandId");
            if (!await _masterLookup.IsCategoryValidAsync(model.CategoryId)) throw new ArgumentException("Invalid CategoryId");
            if (!await _masterLookup.IsProductTypeValidAsync(model.ProductTypeId)) throw new ArgumentException("Invalid ProductTypeId");
            if (!await _masterLookup.IsGoldKaratValidAsync(model.GoldTypeId)) throw new ArgumentException("Invalid GoldTypeId");
            if (model.CertificateId.HasValue && !await _masterLookup.IsCertificateValidAsync(model.CertificateId.Value)) 
                throw new ArgumentException("Invalid CertificateId");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating new product with StyleCode: {StyleCode} by user: {UserId}", model.StyleCode, userId);

                var item = new Item
                {
                    StyleCode = model.StyleCode,
                    ItemName = model.ItemName,
                    Description = model.Description,
                    Pairs = model.Pairs,
                    Quantity = model.Quantity,
                    BrandId = model.BrandId,
                    CategoryId = model.CategoryId,
                    CertificateId = model.CertificateId,
                    ProductTypeId = model.ProductTypeId,
                    GoldTypeId = model.GoldTypeId,
                    GoldWeight = Math.Round(model.GoldWeight, 3),
                    StoneWeight = Math.Round(model.StoneWeight, 3),
                    WastagePercentage = Math.Round(model.WastagePercentage, 2),
                    GoldRate = Math.Round(model.GoldRate, 2),
                    GoldMakingCharges = Math.Round(model.GoldMakingCharges, 2),
                    StoneMakingCharges = Math.Round(model.StoneMakingCharges, 2),
                    OtherMakingCharges = Math.Round(model.OtherMakingCharges, 2),
                    DiscountPercentage = Math.Round(model.DiscountPercentage, 2),
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    IsNewLaunch = model.IsNewLaunch,
                    IsOnSale = model.IsOnSale,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                // Add Diamond Details
                if (model.DiamondDetails != null && model.DiamondDetails.Any())
                {
                    foreach (var d in model.DiamondDetails)
                    {
                        var diamond = new DiamondDetail
                        {
                            DiamondQualityId = d.DiamondQualityId,
                            Carat = Math.Round(d.Carat, 3),
                            Pieces = d.Pieces,
                            Weight = Math.Round(d.Weight, 3),
                            Rate = Math.Round(d.Rate, 2),
                            Shape = d.Shape,
                            SettingType = d.SettingType,
                            Remarks = d.Remarks,
                            CreatedDate = DateTime.UtcNow
                        };
                        diamond.CalculateTotalAmount();
                        item.DiamondDetails.Add(diamond);
                    }
                }

                // Add Stone Details
                if (model.StoneDetails != null && model.StoneDetails.Any())
                {
                    foreach (var s in model.StoneDetails)
                    {
                        var stone = new StoneDetail
                        {
                            StoneQualityId = s.StoneQualityId,
                            Weight = Math.Round(s.Weight, 3),
                            Pieces = s.Pieces,
                            Carat = Math.Round(s.Carat, 3),
                            Rate = Math.Round(s.Rate, 2),
                            Shape = s.Shape,
                            SettingType = s.SettingType,
                            Treatment = s.Treatment,
                            Remarks = s.Remarks,
                            CreatedDate = DateTime.UtcNow
                        };
                        stone.CalculateTotalAmount();
                        item.StoneDetails.Add(stone);
                    }
                }

                // Handle Images
                item.PrimaryImageUrl = await _imageService.SaveImageAsync(model.PrimaryImage, IImageService.ProductsMain);
                item.SecondaryImageUrl = await _imageService.SaveImageAsync(model.SecondaryImage, IImageService.ProductsGallery);
                item.TertiaryImageUrl = await _imageService.SaveImageAsync(model.TertiaryImage, IImageService.ProductsGallery);

                // Enforce pricing recalculation on server
                item.TotalDiamondAmount = item.DiamondDetails.Sum(d => d.TotalAmount);
                item.TotalStoneAmount = item.StoneDetails.Sum(s => s.TotalAmount);
                item.CalculatePricing();

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Check for low stock
                if (item.Quantity < 5)
                {
                    await _notificationService.CreateNotificationAsync(
                        "Low Stock Alert",
                        $"Product {item.ItemName} ({item.StyleCode}) has low stock: {item.Quantity} remaining.",
                        NotificationType.LowInventoryThreshold,
                        item.StyleCode);
                }

                _logger.LogInformation("Successfully created product with StyleCode: {StyleCode}, MRP: {MRP}, SellingPrice: {SellingPrice}",
                    item.StyleCode, item.MRP, item.SellingPrice);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating product with StyleCode: {StyleCode}", model.StyleCode);
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(ItemEditViewModel model, string? userId)
        {
            // Validate Master Data
            if (!await _masterLookup.IsBrandValidAsync(model.BrandId)) throw new ArgumentException("Invalid BrandId");
            if (!await _masterLookup.IsCategoryValidAsync(model.CategoryId)) throw new ArgumentException("Invalid CategoryId");
            if (!await _masterLookup.IsProductTypeValidAsync(model.ProductTypeId)) throw new ArgumentException("Invalid ProductTypeId");
            if (!await _masterLookup.IsGoldKaratValidAsync(model.GoldTypeId)) throw new ArgumentException("Invalid GoldTypeId");
            if (!await _masterLookup.IsCertificateValidAsync(model.CertificateId)) throw new ArgumentException("Invalid CertificateId");

            var existingItem = await _context.Items.Include(i => i.DiamondDetails).Include(i => i.StoneDetails).FirstOrDefaultAsync(i => i.StyleCode == model.StyleCode);
            if (existingItem == null)
            {
                _logger.LogWarning("Product not found for update - StyleCode: {StyleCode}", model.StyleCode);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating product with StyleCode: {StyleCode} by user: {UserId}", model.StyleCode, userId);

                // Update basic fields
                existingItem.ItemName = model.ItemName;
                existingItem.Description = model.Description;
                existingItem.Pairs = model.Pairs;
                existingItem.Quantity = model.Quantity;
                existingItem.BrandId = model.BrandId;
                existingItem.CategoryId = model.CategoryId;
                existingItem.CertificateId = model.CertificateId;
                existingItem.ProductTypeId = model.ProductTypeId;
                existingItem.GoldTypeId = model.GoldTypeId;
                existingItem.GoldWeight = Math.Round(model.GoldWeight, 3);
                existingItem.StoneWeight = Math.Round(model.StoneWeight, 3);
                existingItem.WastagePercentage = Math.Round(model.WastagePercentage, 2);
                existingItem.GoldRate = Math.Round(model.GoldRate, 2);
                existingItem.GoldMakingCharges = Math.Round(model.GoldMakingCharges, 2);
                existingItem.StoneMakingCharges = Math.Round(model.StoneMakingCharges, 2);
                existingItem.OtherMakingCharges = Math.Round(model.OtherMakingCharges, 2);
                existingItem.DiscountPercentage = Math.Round(model.DiscountPercentage, 2);
                existingItem.IsActive = model.IsActive;
                existingItem.IsFeatured = model.IsFeatured;
                existingItem.IsNewLaunch = model.IsNewLaunch;
                existingItem.IsOnSale = model.IsOnSale;
                existingItem.MetaTitle = model.MetaTitle;
                existingItem.MetaDescription = model.MetaDescription;
                existingItem.MetaKeywords = model.MetaKeywords;
                existingItem.ModifiedBy = userId;
                existingItem.ModifiedDate = DateTime.UtcNow;

                // Handle Image Updates
                if (model.NewPrimaryImage != null)
                {
                    _imageService.DeleteImage(existingItem.PrimaryImageUrl);
                    existingItem.PrimaryImageUrl = await _imageService.SaveImageAsync(model.NewPrimaryImage, IImageService.ProductsMain);
                    _logger.LogInformation("Updated primary image for product: {StyleCode}", model.StyleCode);
                }
                if (model.NewSecondaryImage != null)
                {
                    _imageService.DeleteImage(existingItem.SecondaryImageUrl);
                    existingItem.SecondaryImageUrl = await _imageService.SaveImageAsync(model.NewSecondaryImage, IImageService.ProductsGallery);
                    _logger.LogInformation("Updated secondary image for product: {StyleCode}", model.StyleCode);
                }
                if (model.NewTertiaryImage != null)
                {
                    _imageService.DeleteImage(existingItem.TertiaryImageUrl);
                    existingItem.TertiaryImageUrl = await _imageService.SaveImageAsync(model.NewTertiaryImage, IImageService.ProductsGallery);
                    _logger.LogInformation("Updated tertiary image for product: {StyleCode}", model.StyleCode);
                }

                // Sync Diamond Details
                var diamondDetailsToUpdate = model.DiamondDetails ?? new List<DiamondDetailViewModel>();
                
                // Remove deleted ones
                var diamondIdsToKeep = diamondDetailsToUpdate.Where(d => !d.IsDeleted && d.DiamondDetailId > 0).Select(d => d.DiamondDetailId).ToList();
                var diamondsToRemove = existingItem.DiamondDetails.Where(d => !diamondIdsToKeep.Contains(d.DiamondDetailId)).ToList();
                foreach (var d in diamondsToRemove) _context.DiamondDetails.Remove(d);

                // Add or update
                foreach (var d in diamondDetailsToUpdate.Where(d => !d.IsDeleted))
                {
                    if (d.DiamondDetailId > 0)
                    {
                        var existingD = existingItem.DiamondDetails.FirstOrDefault(ex => ex.DiamondDetailId == d.DiamondDetailId);
                        if (existingD != null)
                        {
                            existingD.DiamondQualityId = d.DiamondQualityId;
                            existingD.Carat = Math.Round(d.Carat, 3);
                            existingD.Pieces = d.Pieces;
                            existingD.Weight = Math.Round(d.Weight, 3);
                            existingD.Rate = Math.Round(d.Rate, 2);
                            existingD.Shape = d.Shape;
                            existingD.SettingType = d.SettingType;
                            existingD.Remarks = d.Remarks;
                            existingD.ModifiedDate = DateTime.UtcNow;
                            existingD.CalculateTotalAmount();
                        }
                    }
                    else
                    {
                        var newD = new DiamondDetail
                        {
                            StyleCode = existingItem.StyleCode,
                            DiamondQualityId = d.DiamondQualityId,
                            Carat = Math.Round(d.Carat, 3),
                            Pieces = d.Pieces,
                            Weight = Math.Round(d.Weight, 3),
                            Rate = Math.Round(d.Rate, 2),
                            Shape = d.Shape,
                            SettingType = d.SettingType,
                            Remarks = d.Remarks,
                            CreatedDate = DateTime.UtcNow
                        };
                        newD.CalculateTotalAmount();
                        existingItem.DiamondDetails.Add(newD);
                    }
                }

                // Sync Stone Details
                var stoneDetailsToUpdate = model.StoneDetails ?? new List<StoneDetailViewModel>();
                
                // Remove deleted ones
                var stoneIdsToKeep = stoneDetailsToUpdate.Where(s => !s.IsDeleted && s.StoneDetailId > 0).Select(s => s.StoneDetailId).ToList();
                var stonesToRemove = existingItem.StoneDetails.Where(s => !stoneIdsToKeep.Contains(s.StoneDetailId)).ToList();
                foreach (var s in stonesToRemove) _context.StoneDetails.Remove(s);

                // Add or update
                foreach (var s in stoneDetailsToUpdate.Where(s => !s.IsDeleted))
                {
                    if (s.StoneDetailId > 0)
                    {
                        var existingS = existingItem.StoneDetails.FirstOrDefault(ex => ex.StoneDetailId == s.StoneDetailId);
                        if (existingS != null)
                        {
                            existingS.StoneQualityId = s.StoneQualityId;
                            existingS.Weight = Math.Round(s.Weight, 3);
                            existingS.Pieces = s.Pieces;
                            existingS.Carat = Math.Round(s.Carat, 3);
                            existingS.Rate = Math.Round(s.Rate, 2);
                            existingS.Shape = s.Shape;
                            existingS.SettingType = s.SettingType;
                            existingS.Treatment = s.Treatment;
                            existingS.Remarks = s.Remarks;
                            existingS.ModifiedDate = DateTime.UtcNow;
                            existingS.CalculateTotalAmount();
                        }
                    }
                    else
                    {
                        var newS = new StoneDetail
                        {
                            StyleCode = existingItem.StyleCode,
                            StoneQualityId = s.StoneQualityId,
                            Weight = Math.Round(s.Weight, 3),
                            Pieces = s.Pieces,
                            Carat = Math.Round(s.Carat, 3),
                            Rate = Math.Round(s.Rate, 2),
                            Shape = s.Shape,
                            SettingType = s.SettingType,
                            Treatment = s.Treatment,
                            Remarks = s.Remarks,
                            CreatedDate = DateTime.UtcNow
                        };
                        newS.CalculateTotalAmount();
                        existingItem.StoneDetails.Add(newS);
                    }
                }

                // Recalculate Pricing
                existingItem.TotalDiamondAmount = existingItem.DiamondDetails.Sum(d => d.TotalAmount);
                existingItem.TotalStoneAmount = existingItem.StoneDetails.Sum(s => s.TotalAmount);
                existingItem.CalculatePricing();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully updated product: {StyleCode}, New MRP: {MRP}, New SellingPrice: {SellingPrice}",
                    existingItem.StyleCode, existingItem.MRP, existingItem.SellingPrice);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating product with StyleCode: {StyleCode}", model.StyleCode);
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(string styleCode)
        {
            try
            {
                _logger.LogInformation("Attempting to delete product with StyleCode: {StyleCode}", styleCode);

                var item = await _context.Items
                    .Include(i => i.OrderItems)
                    .FirstOrDefaultAsync(i => i.StyleCode == styleCode);

                if (item == null)
                {
                    _logger.LogWarning("Product not found for deletion - StyleCode: {StyleCode}", styleCode);
                    return false;
                }

                if (item.OrderItems.Any())
                {
                    // Soft delete only
                    _logger.LogInformation("Product has {Count} order items, performing soft delete - StyleCode: {StyleCode}",
                        item.OrderItems.Count, styleCode);
                    item.IsActive = false;
                    item.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Safe to delete completely
                _logger.LogInformation("Product has no orders, performing hard delete - StyleCode: {StyleCode}", styleCode);
                _imageService.DeleteImage(item.PrimaryImageUrl);
                _imageService.DeleteImage(item.SecondaryImageUrl);
                _imageService.DeleteImage(item.TertiaryImageUrl);

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted product: {StyleCode}", styleCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with StyleCode: {StyleCode}", styleCode);
                throw;
            }
        }

        public async Task<bool> StyleCodeExistsAsync(string styleCode)
        {
            return await _context.Items.AnyAsync(i => i.StyleCode == styleCode);
        }

        public async Task<bool> ToggleStatusAsync(string styleCode)
        {
            try
            {
                _logger.LogInformation("Toggling status for product: {StyleCode}", styleCode);
                var item = await _context.Items.FirstOrDefaultAsync(i => i.StyleCode == styleCode);
                if (item == null)
                {
                    _logger.LogWarning("Product not found for status toggle: {StyleCode}", styleCode);
                    return false;
                }

                item.IsActive = !item.IsActive;
                item.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully toggled status for product: {StyleCode} to {Status}", styleCode, item.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for product: {StyleCode}", styleCode);
                throw;
            }
        }

        private IQueryable<Item> GetFilteredQuery(string? searchTerm, int? brandId, int? categoryId, bool? isActive)
        {
            var query = _context.Items.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(i => i.StyleCode.ToLower().Contains(searchTerm) || 
                                         i.ItemName.ToLower().Contains(searchTerm) ||
                                         (i.Brand != null && i.Brand.BrandType.ToLower().Contains(searchTerm)) ||
                                         (i.Category != null && i.Category.CategoryName.ToLower().Contains(searchTerm)));
            }

            if (brandId.HasValue) query = query.Where(i => i.BrandId == brandId.Value);
            if (categoryId.HasValue) query = query.Where(i => i.CategoryId == categoryId.Value);
            if (isActive.HasValue) query = query.Where(i => i.IsActive == isActive.Value);

            return query;
        }
    }
}



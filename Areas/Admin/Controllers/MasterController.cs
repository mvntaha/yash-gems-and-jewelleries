using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yash_Gems___Jewelleries.Models.Masters;
using Yash_Gems___Jewelleries.ViewModels;
using Yash_Gems___Jewelleries.Repositories;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.Models.Interfaces;
using Yash_Gems___Jewelleries.Models.Metadata;
using System.Linq.Expressions;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    /// <summary>
    /// Unified Master Data Controller using Generic Repository Pattern.
    /// Handles CRUD operations for all master entities through entity type discrimination.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MasterController : Controller
    {
        private readonly IEntityMetadataProvider _metadataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IImageService _imageService;
        private readonly ILogger<MasterController> _logger;

        public MasterController(
            IEntityMetadataProvider metadataProvider,
            IServiceProvider serviceProvider,
            IImageService imageService,
            ILogger<MasterController> logger)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private dynamic? GetRepository(string entityType)
        {
            if (string.IsNullOrEmpty(entityType)) return null;
            var metadata = _metadataProvider.GetMetadata(entityType);
            if (metadata == null) return null;

            var repositoryType = typeof(IMasterDataRepository<>).MakeGenericType(metadata.EntityClass);
            return _serviceProvider.GetService(repositoryType);
        }

        // GET: Admin/Master/List?entityType=brand
        [HttpGet]
        public async Task<IActionResult> List(string entityType, bool? isActive = null)
        {
            try
            {
                var metadata = _metadataProvider.GetMetadata(entityType);
                if (metadata == null) return NotFound("Invalid entity type.");

                var repo = GetRepository(entityType);
                if (repo == null) return NotFound("Repository not found.");

                dynamic items;
                string typeLower = entityType.ToLower();

                if (typeLower == "certificate")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<Certificate, object>>)(c => c.Items));
                }
                else if (typeLower == "category")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<Category, object>>)(c => c.Items));
                }
                else if (typeLower == "producttype")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<ProductType, object>>)(c => c.Items));
                }
                else if (typeLower == "brand")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<Brand, object>>)(c => c.Items));
                }
                else if (typeLower == "goldkarat")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<GoldKarat, object>>)(c => c.Items));
                }
                else if (typeLower == "stonequality")
                {
                    items = await repo.GetAllAsync(isActive, (Expression<Func<StoneQuality, object>>)(c => c.StoneDetails));
                }
                else
                {
                    items = await repo.GetAllAsync(isActive);
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(metadata.TableView, items);
                }

                ViewBag.EntityType = entityType;
                ViewBag.Metadata = metadata;
                return View("Master", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing items for {EntityType}", entityType);
                return BadRequest("An error occurred while loading data.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetModal([FromQuery] string entityType, [FromQuery] int? id)
        {
            try
            { 
                var metadata = _metadataProvider.GetMetadata(entityType);
                if (metadata == null) return NotFound("Invalid entity type.");

                var repo = GetRepository(entityType);
                if (repo == null) return NotFound("Repository not found.");

                object? model = null;
                
                // Use reflection to create the ViewModel or resolve it
                // For simplicity now, we can use a switch or try to find it by convention
                model = await GetEntityViewModel(entityType, id);

                if (model == null) return NotFound();

                return PartialView(metadata.FormPartial, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting modal for {EntityType} with ID {Id}", entityType, id);
                return BadRequest("An error occurred while loading the form.");
            }
        }

        // POST: Admin/Master/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus([FromForm] string entityType, [FromForm] int id)
        {
            try
            {
                var repo = GetRepository(entityType);
                if (repo == null) return Json(new { success = false, message = "Invalid entity type" });

                var entity = await repo.GetByIdAsync(id);
                if (entity == null) return Json(new { success = false, message = "Record not found" });

                if (entity is IMasterEntity masterEntity)
                {
                    masterEntity.IsActive = !masterEntity.IsActive;
                    masterEntity.ModifiedDate = DateTime.UtcNow;
                    await repo.UpdateAsync(entity);
                    return Json(new { success = true, message = $"Status updated to {(masterEntity.IsActive ? "Active" : "Inactive")}.", type = "success", isActive = masterEntity.IsActive });
                }

                return Json(new { success = false, message = "Entity does not support status toggle" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for {EntityType} with ID {Id}", entityType, id);
                return Json(new { success = false, message = "An error occurred while updating status.", type = "danger" });
            }
        }

        // POST: Admin/Master/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] string entityType)
        {
            if (string.IsNullOrEmpty(entityType))
            {
                _logger.LogError("Model bound but entityType is null. Form values were not captured.");
            }

            try
            {
                var metadata = _metadataProvider.GetMetadata(entityType);
                if (metadata == null) return NotFound("Invalid entity type.");

                var repo = GetRepository(entityType);
                if (repo == null) return NotFound("Repository not found.");

                // Bind to appropriate ViewModel and handle Save logic
                return await ProcessSave(entityType, metadata, repo);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "The record was modified by another user. Please refresh and try again.");
                return await GetFormWithModelOnError(entityType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving {EntityType}", entityType);
                ModelState.AddModelError("", "An error occurred while saving.");
                return await GetFormWithModelOnError(entityType);
            }
        }

        // POST: Admin/Master/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string entityType, [FromForm] int id)
        {
            try
            {
                var repo = GetRepository(entityType);
                if (repo == null) return Json(new { success = false, message = "Repository not found" });

                var entity = await repo.GetByIdAsync(id);
                if (entity == null) return Json(new { success = false, message = "Record not found" });

                await repo.DeleteAsync(id);
                return Json(new { success = true, message = "Record deleted successfully", type = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", entityType, id);
                return Json(new { success = false, message = "An error occurred while deleting the record", type = "danger" });
            }
        }

        private async Task<IActionResult> ProcessSave(string entityType, EntityMetadata metadata, dynamic repo)
        {
            switch (entityType.ToLower())
            {
                case "brand":
                    var brandVm = new BrandViewModel();
                    if (!await TryUpdateModelAsync(brandVm)) return PartialView(metadata.FormPartial, brandVm);

                    if (await repo.UniqueFieldExistsAsync(nameof(Brand.BrandType), brandVm.BrandType, brandVm.BrandId > 0 ? (int?)brandVm.BrandId : null))
                    {
                        ModelState.AddModelError("BrandType", "A brand with this name already exists.");
                        return PartialView(metadata.FormPartial, brandVm);
                    }

                    string? logoUrl = brandVm.ExistingLogoUrl;
                    if (brandVm.LogoFile != null)
                    {
                        if (!string.IsNullOrEmpty(brandVm.ExistingLogoUrl)) _imageService.DeleteImage(brandVm.ExistingLogoUrl);
                        logoUrl = await _imageService.SaveImageAsync(brandVm.LogoFile, IImageService.Brands);
                    }

                    if (brandVm.BrandId > 0)
                    {
                        var existing = await repo.GetByIdAsync(brandVm.BrandId);
                        if (existing == null) return NotFound();
                        existing.BrandType = brandVm.BrandType;
                        existing.Description = brandVm.Description;
                        existing.Website = brandVm.Website;
                        existing.Location = brandVm.Location;
                        existing.Email = brandVm.Email;
                        existing.PhoneNumber = brandVm.PhoneNumber;
                        existing.IsActive = brandVm.IsActive;
                        if (logoUrl != null) existing.LogoUrl = logoUrl;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new Brand { BrandType = brandVm.BrandType, Description = brandVm.Description, Website = brandVm.Website, Location = brandVm.Location, Email = brandVm.Email, PhoneNumber = brandVm.PhoneNumber, IsActive = brandVm.IsActive, LogoUrl = logoUrl });
                    }
                    break;

                case "category":
                    var catVm = new CategoryViewModel();
                    if (!await TryUpdateModelAsync(catVm)) return PartialView(metadata.FormPartial, catVm);
                    
                    string? catImg = catVm.ExistingImageUrl;
                    if (catVm.ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(catVm.ExistingImageUrl)) _imageService.DeleteImage(catVm.ExistingImageUrl);
                        catImg = await _imageService.SaveImageAsync(catVm.ImageFile, IImageService.Categories);
                    }

                    if (catVm.CategoryId > 0)
                    {
                        var existing = await repo.GetByIdAsync(catVm.CategoryId);
                        if (existing == null) return NotFound();
                        existing.CategoryName = catVm.CategoryName;
                        existing.Description = catVm.Description;
                        existing.Stock = catVm.Stock;
                        existing.IsActive = catVm.IsActive;
                        existing.ImageUrl = catImg;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new Category { CategoryName = catVm.CategoryName, Description = catVm.Description, Stock = catVm.Stock, IsActive = catVm.IsActive, ImageUrl = catImg });
                    }
                    break;

                case "certificate":
                    var certVm = new CertificateViewModel();
                    if (!await TryUpdateModelAsync(certVm)) return PartialView(metadata.FormPartial, certVm);

                    if (certVm.CertificateId > 0)
                    {
                        var existing = await repo.GetByIdAsync(certVm.CertificateId);
                        if (existing == null) return NotFound();
                        existing.CertifyType = certVm.CertifyType;
                        existing.Description = certVm.Description;
                        existing.IssuingAuthority = certVm.IssuingAuthority;
                        existing.IsActive = certVm.IsActive;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new Certificate { CertifyType = certVm.CertifyType, Description = certVm.Description, IssuingAuthority = certVm.IssuingAuthority, IsActive = certVm.IsActive });
                    }
                    break;

                case "producttype":
                    var ptVm = new ProductTypeViewModel();
                    if (!await TryUpdateModelAsync(ptVm)) return PartialView(metadata.FormPartial, ptVm);
                    
                    string? ptImg = ptVm.ExistingImageUrl;
                    if (ptVm.ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(ptVm.ExistingImageUrl)) _imageService.DeleteImage(ptVm.ExistingImageUrl);
                        ptImg = await _imageService.SaveImageAsync(ptVm.ImageFile, IImageService.ProductsGallery); // Mapping ProductType images to gallery or main as appropriate
                    }

                    if (ptVm.ProductTypeId > 0)
                    {
                        var existing = await repo.GetByIdAsync(ptVm.ProductTypeId);
                        if (existing == null) return NotFound();
                        existing.ProductTypeName = ptVm.ProductTypeName;
                        existing.Description = ptVm.Description;
                        existing.IsActive = ptVm.IsActive;
                        existing.ImageUrl = ptImg;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new ProductType { ProductTypeName = ptVm.ProductTypeName, Description = ptVm.Description, IsActive = ptVm.IsActive, ImageUrl = ptImg });
                    }
                    break;

                case "diamondquality":
                    var dqVm = new DiamondQualityViewModel();
                    if (!await TryUpdateModelAsync(dqVm)) return PartialView(metadata.FormPartial, dqVm);

                    if (dqVm.Id > 0)
                    {
                        var existing = await repo.GetByIdAsync(dqVm.Id);
                        if (existing == null) return NotFound();
                        existing.QualityGrade = dqVm.QualityGrade;
                        existing.Description = dqVm.Description;
                        existing.Clarity = dqVm.Clarity;
                        existing.Color = dqVm.Color;
                        existing.Cut = dqVm.Cut;
                        existing.IsActive = dqVm.IsActive;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new DiamondQuality { QualityGrade = dqVm.QualityGrade, Description = dqVm.Description, Clarity = dqVm.Clarity, Color = dqVm.Color, Cut = dqVm.Cut, IsActive = dqVm.IsActive });
                    }
                    break;

                case "stonequality":
                    var sqVm = new StoneQualityViewModel();
                    if (!await TryUpdateModelAsync(sqVm)) return PartialView(metadata.FormPartial, sqVm);

                    if (sqVm.Id > 0)
                    {
                        var existing = await repo.GetByIdAsync(sqVm.Id);
                        if (existing == null) return NotFound();
                        existing.StoneType = sqVm.StoneType;
                        existing.QualityGrade = sqVm.QualityGrade;
                        existing.Description = sqVm.Description;
                        existing.Color = sqVm.Color;
                        existing.Origin = sqVm.Origin;
                        existing.IsActive = sqVm.IsActive;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new StoneQuality { StoneType = sqVm.StoneType, QualityGrade = sqVm.QualityGrade, Description = sqVm.Description, Color = sqVm.Color, Origin = sqVm.Origin, IsActive = sqVm.IsActive });
                    }
                    break;

                case "goldkarat":
                    var gkVm = new GoldKaratViewModel();
                    if (!await TryUpdateModelAsync(gkVm)) return PartialView(metadata.FormPartial, gkVm);

                    if (gkVm.Id > 0)
                    {
                        var existing = await repo.GetByIdAsync(gkVm.Id);
                        if (existing == null) return NotFound();
                        existing.GoldCarat = gkVm.GoldCarat;
                        existing.PurityPercentage = gkVm.PurityPercentage;
                        existing.Description = gkVm.Description;
                        existing.IsActive = gkVm.IsActive;
                        await repo.UpdateAsync(existing);
                    }
                    else
                    {
                        await repo.CreateAsync(new GoldKarat { GoldCarat = gkVm.GoldCarat, PurityPercentage = gkVm.PurityPercentage, Description = gkVm.Description, IsActive = gkVm.IsActive });
                    }
                    break;

                default:
                    return Json(new { success = false, message = "Unsupported entity type", type = "warning" });
            }

            return Json(new { success = true, message = "Saved successfully", type = "success" });
        }

        private async Task<IActionResult> GetFormWithModelOnError(string entityType)
        {
            if (string.IsNullOrEmpty(entityType)) return BadRequest("Entity type is required.");

            var metadata = _metadataProvider.GetMetadata(entityType);
            if (metadata == null) return NotFound();

            // Try to re-bind or return a fresh model to prevent null errors in partials
            var model = await GetEntityViewModel(entityType, null);
            return PartialView(metadata.FormPartial, model);
        }

        private async Task<object?> GetEntityViewModel(string entityType, int? id)
        {
            var repo = GetRepository(entityType);
            if (repo == null) return null;
            
            switch (entityType.ToLower())
            {
                case "brand":
                    var brand = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return brand != null ? new BrandViewModel {
                        BrandId = brand.BrandId,
                        BrandType = brand.BrandType,
                        Description = brand.Description,
                        Website = brand.Website,
                        Location = brand.Location,
                        Email = brand.Email,
                        PhoneNumber = brand.PhoneNumber,
                        IsActive = brand.IsActive,
                        ExistingLogoUrl = brand.LogoUrl,
                        RowVersion = brand.RowVersion
                    } : new BrandViewModel();

                case "category":
                    var category = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return category != null ? new CategoryViewModel {
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName,
                        Description = category.Description,
                        Stock = category.Stock,
                        IsActive = category.IsActive,
                        ExistingImageUrl = category.ImageUrl,
                        RowVersion = category.RowVersion
                    } : new CategoryViewModel();

                case "certificate":
                    var certificate = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return certificate != null ? new CertificateViewModel {
                        CertificateId = certificate.CertificateId,
                        CertifyType = certificate.CertifyType,
                        Description = certificate.Description,
                        IssuingAuthority = certificate.IssuingAuthority,
                        IsActive = certificate.IsActive,
                        RowVersion = certificate.RowVersion
                    } : new CertificateViewModel();

                case "producttype":
                    var pt = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return pt != null ? new ProductTypeViewModel {
                        ProductTypeId = pt.ProductTypeId,
                        ProductTypeName = pt.ProductTypeName,
                        Description = pt.Description,
                        IsActive = pt.IsActive,
                        ExistingImageUrl = pt.ImageUrl,
                        RowVersion = pt.RowVersion
                    } : new ProductTypeViewModel();

                case "diamondquality":
                    var dq = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return dq != null ? new DiamondQualityViewModel {
                        Id = dq.DiamondQualityId,
                        QualityGrade = dq.QualityGrade,
                        Description = dq.Description,
                        Clarity = dq.Clarity,
                        Color = dq.Color,
                        Cut = dq.Cut,
                        IsActive = dq.IsActive,
                        RowVersion = dq.RowVersion
                    } : new DiamondQualityViewModel();

                case "stonequality":
                    var sq = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return sq != null ? new StoneQualityViewModel {
                        Id = sq.StoneQualityId,
                        StoneType = sq.StoneType,
                        QualityGrade = sq.QualityGrade,
                        Description = sq.Description,
                        Color = sq.Color,
                        Origin = sq.Origin,
                        IsActive = sq.IsActive,
                        RowVersion = sq.RowVersion
                    } : new StoneQualityViewModel();

                case "goldkarat":
                    var gk = id.HasValue && id.Value > 0 ? await repo.GetByIdAsync(id.Value) : null;
                    return gk != null ? new GoldKaratViewModel {
                        Id = gk.GoldTypeId,
                        GoldCarat = gk.GoldCarat,
                        PurityPercentage = gk.PurityPercentage,
                        Description = gk.Description,
                        IsActive = gk.IsActive,
                        RowVersion = gk.RowVersion
                    } : new GoldKaratViewModel();

                default:
                    return null;
            }
        }

        // Combined Master Management Page
        [HttpGet]
        public IActionResult Index(string? type = null)
        {
            ViewBag.InitialType = type;
            return View("Master");
        }
    }
}



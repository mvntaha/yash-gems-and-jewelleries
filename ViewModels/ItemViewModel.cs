using System.ComponentModel.DataAnnotations;
using Yash_Gems___Jewelleries.Models;
using System.Collections.Generic;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Item Type View Model
    public class ItemTypeViewModel
    {
        public int ProductTypeId { get; set; }

        [Required(ErrorMessage = "Product type name is required")]
        [StringLength(100, ErrorMessage = "Product type name cannot exceed 100 characters")]
        [Display(Name = "Product Type Name")]
        public string ProductTypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Product Type Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }

    // Item Filter View Model
    public class ItemFilterViewModel
    {
        // Selected filters
        public int[]? BrandIds { get; set; }
        public int[]? CategoryIds { get; set; }
        public int[]? GoldTypeIds { get; set; }
        public string[]? StoneColors { get; set; }
        public int[]? ProductTypeIds { get; set; }
        public string[]? DiamondClarities { get; set; }
        public string[]? DiamondColors { get; set; }
        public string[]? DiamondCuts { get; set; }
        public string? CaratRange { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Availability { get; set; } // "inStock", "outStock"
        public string? SortBy { get; set; }
        public string? SearchQuery { get; set; }
        public int Page { get; set; } = 1;

        // Display Data & Counts
        public List<ItemFilterOption> Categories { get; set; } = new();
        public List<ItemFilterOption> Brands { get; set; } = new();
        public List<ItemFilterOption> ProductTypes { get; set; } = new();
        public List<ItemFilterOption> Materials { get; set; } = new();
        public List<ItemFilterOption> StoneColorsList { get; set; } = new();
        public List<ItemFilterOption> DiamondClaritiesList { get; set; } = new();
        public List<ItemFilterOption> DiamondColorsList { get; set; } = new();
        public List<ItemFilterOption> DiamondCutsList { get; set; } = new();
        
        public int InStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        
        // Additional properties for view compatibility
        public int CurrentPage => Page;
        public int TotalCount => TotalItems;
        public int[] SelectedProductTypeIds => ProductTypeIds ?? Array.Empty<int>();
        public string[] SelectedDiamondClarities => DiamondClarities ?? Array.Empty<string>();
        public string[] SelectedDiamondColors => DiamondColors ?? Array.Empty<string>();
        public List<string> AvailableDiamondClarities => DiamondClaritiesList.Select(x => x.Label).ToList();
        public List<string> AvailableDiamondColors => DiamondColorsList.Select(x => x.Label).ToList();
        public string? PriceRange
        {
            get
            {
                if (MinPrice.HasValue && MaxPrice.HasValue)
                {
                    return $"{MinPrice}-{MaxPrice}";
                }
                return null;
            }
        }
        
        // The actual items for the current page
        public IEnumerable<Item> Items { get; set; } = new List<Item>();
        public List<string> WishlistStyleCodes { get; set; } = new();
    }

    // Item Quick View Model
    public class ItemQuickViewViewModel
    {
        public Item? Item { get; set; }
        public bool IsWishlisted { get; set; }
    }

    // Item Filter Option Model
    public class ItemFilterOption
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsSelected { get; set; }
        
        // Aliases for view compatibility
        public string Name => Label;
        public int ProductCount => Count;
    }

    // Diamond Detail View Model
    public class DiamondDetailViewModel
    {
        public int DiamondDetailId { get; set; }

        [Required(ErrorMessage = "Diamond quality is required")]
        [Display(Name = "Diamond Quality")]
        public int DiamondQualityId { get; set; }

        [Required(ErrorMessage = "Carat is required")]
        [Range(0.001, 100.000, ErrorMessage = "Carat must be greater than 0")]
        public decimal Carat { get; set; }

        [Required(ErrorMessage = "Pieces is required")]
        [Range(1, 10000, ErrorMessage = "Pieces must be at least 1")]
        public int Pieces { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.001, 1000.000, ErrorMessage = "Weight must be greater than 0")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Rate is required")]
        [Range(0, 10000000)]
        public decimal Rate { get; set; }

        [StringLength(100)]
        public string? Shape { get; set; }

        [StringLength(100)]
        [Display(Name = "Setting Type")]
        public string? SettingType { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public bool IsDeleted { get; set; }
    }

    // Stone Detail View Model
    public class StoneDetailViewModel
    {
        public int StoneDetailId { get; set; }

        [Required(ErrorMessage = "Stone quality is required")]
        [Display(Name = "Stone Quality")]
        public int StoneQualityId { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.001, 1000.000)]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Pieces is required")]
        [Range(1, 10000)]
        public int Pieces { get; set; }

        [Range(0, 1000.000)]
        public decimal Carat { get; set; }

        [Required(ErrorMessage = "Rate is required")]
        [Range(0, 10000000)]
        public decimal Rate { get; set; }

        [StringLength(100)]
        public string? Shape { get; set; }

        [StringLength(100)]
        [Display(Name = "Setting Type")]
        public string? SettingType { get; set; }

        [StringLength(50)]
        public string? Treatment { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public bool IsDeleted { get; set; }
    }

    // Item Create View Model
    public class ItemCreateViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Style Code")]
        public string StyleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Pairs { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Available Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Certificate")]
        public int? CertificateId { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public int ProductTypeId { get; set; }

        [Required]
        [Display(Name = "Gold Type")]
        public int GoldTypeId { get; set; }

        [Required]
        [Display(Name = "Gold Weight (grams)")]
        [Range(0.001, 10000)]
        public decimal GoldWeight { get; set; }

        [Display(Name = "Stone Weight (grams)")]
        [Range(0, 10000.000)]
        public decimal StoneWeight { get; set; } = 0;

        [Required]
        [Display(Name = "Wastage Percentage")]
        [Range(0, 100)]
        public decimal WastagePercentage { get; set; }

        [Required]
        [Display(Name = "Gold Rate (per gram)")]
        [Range(0, 1000000)]
        public decimal GoldRate { get; set; }

        [Display(Name = "Gold Making Charges")]
        [Range(0, 1000000)]
        public decimal GoldMakingCharges { get; set; } = 0;

        [Display(Name = "Stone Making Charges")]
        [Range(0, 1000000)]
        public decimal StoneMakingCharges { get; set; } = 0;

        [Display(Name = "Other Making Charges")]
        [Range(0, 1000000)]
        public decimal OtherMakingCharges { get; set; } = 0;

        [Display(Name = "Discount Percentage")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNewLaunch { get; set; } = false;
        public bool IsOnSale { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        [Display(Name = "Meta Keywords")]
        public string? MetaKeywords { get; set; }

        public IFormFile? PrimaryImage { get; set; }
        public IFormFile? SecondaryImage { get; set; }
        public IFormFile? TertiaryImage { get; set; }

        // Nested Details
        public List<DiamondDetailViewModel> DiamondDetails { get; set; } = new();
        public List<StoneDetailViewModel> StoneDetails { get; set; } = new();
    }

    // Item Edit View Model
    public class ItemEditViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Style Code")]
        public string StyleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Pairs { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Available Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Certificate")]
        public int? CertificateId { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public int ProductTypeId { get; set; }

        [Required]
        [Display(Name = "Gold Type")]
        public int GoldTypeId { get; set; }

        [Required]
        [Display(Name = "Gold Weight (grams)")]
        [Range(0.001, 10000.000)]
        public decimal GoldWeight { get; set; }

        [Display(Name = "Stone Weight (grams)")]
        [Range(0, 10000.000)]
        public decimal StoneWeight { get; set; } = 0;

        [Required]
        [Display(Name = "Wastage Percentage")]
        [Range(0, 100)]
        public decimal WastagePercentage { get; set; }

        [Required]
        [Display(Name = "Gold Rate (per gram)")]
        [Range(0, 1000000)]
        public decimal GoldRate { get; set; }

        [Display(Name = "Gold Making Charges")]
        [Range(0, 1000000)]
        public decimal GoldMakingCharges { get; set; } = 0;

        [Display(Name = "Stone Making Charges")]
        [Range(0, 1000000)]
        public decimal StoneMakingCharges { get; set; } = 0;

        [Display(Name = "Other Making Charges")]
        [Range(0, 1000000)]
        public decimal OtherMakingCharges { get; set; } = 0;

        [Display(Name = "Discount Percentage")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNewLaunch { get; set; } = false;
        public bool IsOnSale { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        [Display(Name = "Meta Keywords")]
        public string? MetaKeywords { get; set; }

        // Existing Images for display
        public string? ExistingPrimaryImageUrl { get; set; }
        public string? ExistingSecondaryImageUrl { get; set; }
        public string? ExistingTertiaryImageUrl { get; set; }

        // New Uploads
        public IFormFile? NewPrimaryImage { get; set; }
        public IFormFile? NewSecondaryImage { get; set; }
        public IFormFile? NewTertiaryImage { get; set; }

        // Nested Details
        public List<DiamondDetailViewModel> DiamondDetails { get; set; } = new();
        public List<StoneDetailViewModel> StoneDetails { get; set; } = new();
    }
}

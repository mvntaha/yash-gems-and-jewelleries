using System.Collections.Generic;

namespace Yash_Gems___Jewelleries.ViewModels
{
    public class HomeViewModel
    {
        public List<ProductHomeViewModel> BestProducts { get; set; } = new();
        public BannerHomeViewModel? FirstBanner { get; set; }
        public ProductHomeViewModel? NewLaunchProduct { get; set; }
        public List<CategoryHomeViewModel> FeaturedCollections { get; set; } = new();
        public BannerHomeViewModel? SecondBanner { get; set; }
        public List<string> WishlistStyleCodes { get; set; } = new();
        public List<DiscountSchemeHomeViewModel> ActiveDiscountSchemes { get; set; } = new();
    }

    public class DiscountSchemeHomeViewModel
    {
        public int DiscountSchemeId { get; set; }
        public string SchemeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public string? CouponCode { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProductHomeViewModel
    {
        public string StyleCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public string? SecondaryImageUrl { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public int Pair { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
    }

    public class BannerHomeViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public string? ButtonText { get; set; }
        public string BannerType { get; set; } = string.Empty;
        public string? DiscountMessage { get; set; }
    }

    public class CategoryHomeViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int ItemCount { get; set; }
    }
}

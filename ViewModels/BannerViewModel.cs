using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.ViewModels
{
    /// <summary>
    /// ViewModel for Banner CRUD operations
    /// </summary>
    public class BannerViewModel : MasterEntityViewModelBase
    {
        public int BannerId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Banner title is required")]
        [StringLength(200)]
        [Display(Name = "Banner Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Button Text")]
        public string? ButtonText { get; set; }

        [StringLength(500)]
        [Display(Name = "Button Link")]
        public string? ButtonLink { get; set; }

        [Required(ErrorMessage = "Display order is required")]
        [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999")]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Required]
        [Display(Name = "Banner Type")]
        public BannerType BannerType { get; set; } = BannerType.NewIn;

        [Display(Name = "Discount Scheme")]
        public int? DiscountSchemeId { get; set; }

        public IEnumerable<SelectListItem>? DiscountSchemes { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Banner Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Mobile Image")]
        public IFormFile? MobileImageFile { get; set; }

        public string? ExistingMobileImageUrl { get; set; }
    }
}

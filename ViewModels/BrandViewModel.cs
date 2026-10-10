using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Brand View Model
    public class BrandViewModel : MasterEntityViewModelBase
    {
        public int BrandId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100)]
        [Display(Name = "Brand Name")]
        public string BrandType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Brand Logo")]
        public IFormFile? LogoFile { get; set; }

        public string? ExistingLogoUrl { get; set; }
    }
}

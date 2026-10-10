using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Product Type View Model - also known as ItemTypeViewModel in some contexts
    public class ProductTypeViewModel : MasterEntityViewModelBase
    {
        public int ProductTypeId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Product type name is required")]
        [StringLength(100, ErrorMessage = "Product type name cannot exceed 100 characters")]
        [Display(Name = "Product Type Name")]
        public string ProductTypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Product Type Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}

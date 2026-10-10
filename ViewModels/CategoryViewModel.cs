using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Category View Model
    public class CategoryViewModel : MasterEntityViewModelBase
    {
        public int CategoryId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number")]
        public int Stock { get; set; }

        [Display(Name = "Category Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}

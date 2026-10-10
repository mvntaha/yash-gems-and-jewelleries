using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Stone Quality View Model
    public class StoneQualityViewModel : MasterEntityViewModelBase
    {
        public int StoneQualityId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Stone type is required")]
        [StringLength(100, ErrorMessage = "Stone type cannot exceed 100 characters")]
        [Display(Name = "Stone Type")]
        public string StoneType { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quality grade cannot exceed 50 characters")]
        [Display(Name = "Quality Grade")]
        public string? QualityGrade { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "Origin cannot exceed 50 characters")]
        [Display(Name = "Origin")]
        public string? Origin { get; set; }
    }
}

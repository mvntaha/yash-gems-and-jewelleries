using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Diamond Quality View Model
    public class DiamondQualityViewModel : MasterEntityViewModelBase
    {
        public int DiamondQualityId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Quality grade is required")]
        [StringLength(50, ErrorMessage = "Quality grade cannot exceed 50 characters")]
        [Display(Name = "Quality Grade")]
        public string QualityGrade { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Clarity cannot exceed 50 characters")]
        [Display(Name = "Clarity")]
        public string? Clarity { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "Cut cannot exceed 50 characters")]
        [Display(Name = "Cut")]
        public string? Cut { get; set; }
    }
}

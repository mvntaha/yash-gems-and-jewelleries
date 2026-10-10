using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Gold Karat View Model
    public class GoldKaratViewModel : MasterEntityViewModelBase
    {
        public int GoldTypeId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Gold carat is required")]
        [StringLength(20, ErrorMessage = "Gold carat cannot exceed 20 characters")]
        [Display(Name = "Gold Carat")]
        public string GoldCarat { get; set; } = string.Empty;

        [Required(ErrorMessage = "Purity percentage is required")]
        [Range(0, 100, ErrorMessage = "Purity percentage must be between 0 and 100")]
        [Display(Name = "Purity Percentage")]
        public decimal PurityPercentage { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}

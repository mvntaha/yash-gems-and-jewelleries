using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    public class SettingsViewModel
    {
        [Required(ErrorMessage = "Owner Phone number is required")]
        [Display(Name = "Owner Phone number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string OwnerPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Owner Email is required")]
        [Display(Name = "Owner Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Address is required")]
        [Display(Name = "Full Address")]
        public string FullAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip-Code is required")]
        [Display(Name = "Zip-Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Default Tax Rate is required")]
        [Display(Name = "Default Tax Rate (%)")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal DefaultTaxRate { get; set; }
    }
}

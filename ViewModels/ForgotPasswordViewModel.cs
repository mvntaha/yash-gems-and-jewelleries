using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Forgot Password View Model
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

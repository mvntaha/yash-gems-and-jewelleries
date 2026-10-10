using System.ComponentModel.DataAnnotations;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.ViewModels
{
    public class InquiryReplyViewModel
    {
        public int InquiryId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string OriginalMessage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Response message is required.")]
        [Display(Name = "Your Response")]
        public string AdminResponse { get; set; } = string.Empty;

        public string Status { get; set; } = "New";

        public DateTime CreatedDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
    }

    public class InquiryListItemViewModel
    {
        public int InquiryId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Existing, will keep for compatibility
        public string CustomerEmail { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? OriginalMessage { get; set; }
        public bool HasReplied { get; set; }
    }

    public class InquiryIndexViewModel
    {
        public IEnumerable<InquiryListItemViewModel> Inquiries { get; set; } = new List<InquiryListItemViewModel>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }
}

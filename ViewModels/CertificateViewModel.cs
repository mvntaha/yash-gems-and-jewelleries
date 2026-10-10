using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Certificate View Model
    public class CertificateViewModel : MasterEntityViewModelBase
    {
        public int CertificateId
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "Certificate type is required")]
        [StringLength(100, ErrorMessage = "Certificate type cannot exceed 100 characters")]
        [Display(Name = "Certificate Type")]
        public string CertifyType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Issuing authority cannot exceed 200 characters")]
        [Display(Name = "Issuing Authority")]
        public string? IssuingAuthority { get; set; }
    }
}

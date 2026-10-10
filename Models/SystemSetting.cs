using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Entity to store system-wide settings
    /// </summary>
    [Table("SystemSettings")]
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string OwnerPhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool PricesWithTax { get; set; } = true;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultTaxRate { get; set; } = 3.00m;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}

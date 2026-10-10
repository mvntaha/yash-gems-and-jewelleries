using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for jewelry brands
    /// </summary>
    [Table("Brands")]
    public class Brand : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BrandId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to BrandId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => BrandId;
            set => BrandId = value;
        }

        [Required]
        [StringLength(100)]
        [Display(Name = "Brand Name")]
        public string BrandType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Row version for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation Properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}

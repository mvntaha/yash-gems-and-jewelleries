using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for product types (e.g., Ring, Necklace, Bracelet, Earrings, Bangle)
    /// </summary>
    [Table("ProductTypes")]
    public class ProductType : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductTypeId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to ProductTypeId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => ProductTypeId;
            set => ProductTypeId = value;
        }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product Type")]
        public string ProductTypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

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

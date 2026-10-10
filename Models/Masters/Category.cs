using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for jewelry categories (e.g., Gold, Diamond, Silver)
    /// </summary>
    [Table("Categories")]
    public class Category : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to CategoryId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => CategoryId;
            set => CategoryId = value;
        }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public int Stock { get; set; }

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

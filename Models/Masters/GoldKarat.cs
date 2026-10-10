using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for gold karat types (e.g., 14K, 18K, 22K, 24K)
    /// </summary>
    [Table("GoldKarats")]
    public class GoldKarat : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GoldTypeId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to GoldTypeId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => GoldTypeId;
            set => GoldTypeId = value;
        }

        [Required]
        [StringLength(20)]
        [Display(Name = "Gold Carat")]
        public string GoldCarat { get; set; } = string.Empty;

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Purity Percentage")]
        public decimal PurityPercentage { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

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

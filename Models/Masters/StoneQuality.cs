using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for gemstone quality (e.g., Ruby, Emerald, Sapphire qualities)
    /// </summary>
    [Table("StoneQualities")]
    public class StoneQuality : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StoneQualityId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to StoneQualityId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => StoneQualityId;
            set => StoneQualityId = value;
        }

        [Required]
        [StringLength(100)]
        [Display(Name = "Stone Type")]
        public string StoneType { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Quality Grade")]
        public string? QualityGrade { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Origin { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Row version for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation Properties
        public virtual ICollection<StoneDetail> StoneDetails { get; set; } = new List<StoneDetail>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Models.Masters
{
    /// <summary>
    /// Master table for certification types (e.g., GIA, IGI, AGS)
    /// </summary>
    [Table("Certificates")]
    public class Certificate : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CertificateId { get; set; }

        /// <summary>
        /// IMasterEntity.Id implementation - maps to CertificateId
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => CertificateId;
            set => CertificateId = value;
        }

        [Required]
        [StringLength(100)]
        [Display(Name = "Certificate Type")]
        public string CertifyType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? IssuingAuthority { get; set; }

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

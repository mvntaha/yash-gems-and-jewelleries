using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models.Interfaces;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Homepage banners and promotional content
    /// </summary>
    [Table("Banners")]
    public class Banner : IMasterEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BannerId { get; set; }

        // IMasterEntity implementation
        [NotMapped]
        public int Id
        {
            get => BannerId;
            set => BannerId = value;
        }

        [Required]
        [StringLength(200)]
        [Display(Name = "Banner Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mobile Image URL")]
        public string? MobileImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Link URL")]
        public string? LinkUrl { get; set; }

        [StringLength(100)]
        [Display(Name = "Button Text")]
        public string? ButtonText { get; set; }

        [Required]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Required]
        [Display(Name = "Banner Type")]
        public BannerType BannerType { get; set; } = BannerType.NewIn;

        [Display(Name = "Discount Scheme")]
        public int? DiscountSchemeId { get; set; }

        [ForeignKey("DiscountSchemeId")]
        public virtual DiscountScheme? DiscountScheme { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

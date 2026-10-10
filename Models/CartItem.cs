using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Shopping cart items for users
    /// </summary>
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartItemId { get; set; }

        // Foreign Key to User (Nullable for Guests)
        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(100)]
        public string? GuestId { get; set; }

        // Foreign Key to Item
        [Required]
        [StringLength(50)]
        [Display(Name = "Style Code")]
        public string StyleCode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price at Add")]
        public decimal PriceAtAdd { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(StyleCode))]
        public virtual Item Item { get; set; } = null!;

        /// <summary>
        /// Calculate subtotal based on quantity and price
        /// </summary>
        public void CalculateSubtotal()
        {
            Subtotal = Quantity * PriceAtAdd;
        }
    }
}

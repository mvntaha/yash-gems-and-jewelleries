using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Main product/item table representing jewelry items
    /// Includes gold calculations, wastage, and pricing logic
    /// </summary>
    [Table("Items")]
    public class Item
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Style Code")]
        public string StyleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        // Quantity Information
        [Range(0, int.MaxValue)]
        public int Pairs { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Available Quantity")]
        public int Quantity { get; set; }

        // Foreign Keys to Master Tables
        [Required]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Certificate")]
        public int? CertificateId { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public int ProductTypeId { get; set; }

        [Required]
        [Display(Name = "Gold Type")]
        public int GoldTypeId { get; set; }

        // Weight Calculations (in grams)
        [Required]
        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Gold Weight (grams)")]
        [Range(0, 9999999.999)]
        public decimal GoldWeight { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Stone Weight (grams)")]
        [Range(0, 9999999.999)]
        public decimal StoneWeight { get; set; } = 0;

        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Net Gold Weight (grams)")]
        [Range(0, 9999999.999)]
        public decimal NetGoldWeight { get; set; }

        // Wastage Calculations
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Wastage Percentage")]
        [Range(0, 100)]
        public decimal WastagePercentage { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Wastage Amount (grams)")]
        public decimal WastageAmount { get; set; }

        // Rates (per gram or per piece)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Gold Rate (per gram)")]
        [Range(0, 999999999999.99)]
        public decimal GoldRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Gold Amount")]
        public decimal GoldAmount { get; set; }

        // Making Charges
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Gold Making Charges")]
        [Range(0, 999999999999.99)]
        public decimal GoldMakingCharges { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Stone Making Charges")]
        [Range(0, 999999999999.99)]
        public decimal StoneMakingCharges { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Other Making Charges")]
        [Range(0, 999999999999.99)]
        public decimal OtherMakingCharges { get; set; } = 0;

        // Pricing
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Diamond Amount")]
        public decimal TotalDiamondAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Stone Amount")]
        public decimal TotalStoneAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "MRP (Total Price)")]
        [Range(0, 999999999999.99)]
        public decimal MRP { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Discount Percentage")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        // Product Images
        [StringLength(500)]
        [Display(Name = "Primary Image")]
        public string? PrimaryImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Secondary Image")]
        public string? SecondaryImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Tertiary Image")]
        public string? TertiaryImageUrl { get; set; }

        // Product Status
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Is New Launch")]
        public bool IsNewLaunch { get; set; } = false;

        [Display(Name = "Is On Sale")]
        public bool IsOnSale { get; set; } = false;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        // SEO Fields
        [StringLength(200)]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        public string? MetaKeywords { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(BrandId))]
        public virtual Brand Brand { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey(nameof(CertificateId))]
        public virtual Certificate? Certificate { get; set; }

        [ForeignKey(nameof(ProductTypeId))]
        public virtual ProductType ProductType { get; set; } = null!;

        [ForeignKey(nameof(GoldTypeId))]
        public virtual GoldKarat GoldKarat { get; set; } = null!;

        public virtual ICollection<DiamondDetail> DiamondDetails { get; set; } = new List<DiamondDetail>();
        public virtual ICollection<StoneDetail> StoneDetails { get; set; } = new List<StoneDetail>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Calculate all derived fields based on input values
        /// Call this method before saving to database
        /// </summary>
        public void CalculatePricing()
        {
            // Calculate wastage amount
            WastageAmount = (GoldWeight * WastagePercentage) / 100;

            // Calculate net gold weight
            NetGoldWeight = GoldWeight + WastageAmount;

            // Calculate gold amount
            GoldAmount = NetGoldWeight * GoldRate;

            // Calculate MRP
            MRP = GoldAmount + GoldMakingCharges + StoneMakingCharges + 
                  OtherMakingCharges + TotalDiamondAmount + TotalStoneAmount;

            // Calculate selling price after discount
            decimal discountAmount = (MRP * DiscountPercentage) / 100;
            SellingPrice = MRP - discountAmount;
        }
    }
}

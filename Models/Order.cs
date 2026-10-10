using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.Models
{
    /// <summary>
    /// Order header table containing overall order information
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        // Foreign Key to User
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Customer Information
        [Required]
        [StringLength(100)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Shipping Address
        [Required]
        [StringLength(200)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "State")]
        public string ShippingState { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "Pakistan";

        // Billing Address
        [Display(Name = "Same as Shipping")]
        public bool BillingAddressSameAsShipping { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "Billing Address")]
        public string? BillingAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "Billing City")]
        public string? BillingCity { get; set; }

        [StringLength(100)]
        [Display(Name = "Billing State")]
        public string? BillingState { get; set; }

        [StringLength(15)]
        [Display(Name = "Billing Postal Code")]
        public string? BillingPostalCode { get; set; }

        [StringLength(100)]
        [Display(Name = "Billing Country")]
        public string? BillingCountry { get; set; }

        // Order Totals
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Shipping Charges")]
        public decimal ShippingCharges { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        // Payment Information
        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty; // CreditCard, DebitCard, UPI, COD, NetBanking

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        // For mock credit card (as per SRS - don't process real payments)
        [StringLength(20)]
        [Display(Name = "Card Number (Last 4 digits)")]
        public string? CardNumberLastFour { get; set; }

        [StringLength(100)]
        [Display(Name = "Card Holder Name")]
        public string? CardHolderName { get; set; }

        // Order Status
        [Required]
        [Display(Name = "Order Status")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [DataType(DataType.DateTime)]
        [Display(Name = "Status Updated At")]
        public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Expected Delivery Date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Actual Delivery Date")]
        public DateTime? ActualDeliveryDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Courier Service")]
        public string? CourierService { get; set; }

        // Additional Information
        [StringLength(1000)]
        [Display(Name = "Customer Notes")]
        public string? CustomerNotes { get; set; }

        [StringLength(1000)]
        [Display(Name = "Admin Notes")]
        public string? AdminNotes { get; set; }

        public bool IsGift { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Gift Message")]
        public string? GiftMessage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();

        [NotMapped]
        public string? ProfilePictureUrl => User?.ProfilePictureUrl;

        /// <summary>
        /// Calculate total amount including tax and shipping
        /// </summary>
        public void CalculateTotalAmount()
        {
            TotalAmount = Subtotal + TaxAmount + ShippingCharges - DiscountAmount;
        }

        /// <summary>
        /// Generate a unique order number with cryptographically secure random component
        /// Format: ORDYYYYMMDDxxxxx (e.g., ORD2026021345678)
        /// </summary>
        public static string GenerateOrderNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomSuffix = RandomNumberGenerator.GetInt32(10000, 99999);
            return $"ORD{datePrefix}{randomSuffix}";
        }
    }
}

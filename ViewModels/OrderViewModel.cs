using System.ComponentModel.DataAnnotations;
using Yash_Gems___Jewelleries.Models.Enums;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Order Placement View Model
    public class OrderPlacementViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email or Phone is required")]
        [Display(Name = "Email or Phone")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        public string? Apartment { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string? State { get; set; }

        [Required(ErrorMessage = "Zip Code is required")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; } = string.Empty;

        public CartViewModel? Cart { get; set; }

        [Display(Name = "Order Notes")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "Please select a shipping method")]
        [Display(Name = "Shipping Method")]
        public string ShippingMethod { get; set; } = "Free";

        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Credit Card";

        // Credit Card Details
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiration Date (MM/YY)")]
        public string? ExpiryDate { get; set; }

        [Display(Name = "Security Code")]
        public string? CVV { get; set; }

        [Display(Name = "Name on Card")]
        public string? CardHolderName { get; set; }

        public bool UseShippingAsBilling { get; set; } = true;

        // Gift Options
        [Display(Name = "Is this a gift?")]
        public bool IsGift { get; set; } = false;

        [StringLength(500, ErrorMessage = "Gift message cannot exceed 500 characters")]
        [Display(Name = "Gift Message")]
        public string? GiftMessage { get; set; }

        // Billing Address (if different from shipping)
        [Display(Name = "First Name")]
        public string? BillingFirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? BillingLastName { get; set; }

        [Display(Name = "Phone")]
        public string? BillingPhone { get; set; }

        [Display(Name = "Address")]
        public string? BillingAddress { get; set; }

        public string? BillingApartment { get; set; }

        [Display(Name = "City")]
        public string? BillingCity { get; set; }

        public string? BillingState { get; set; }

        [Display(Name = "Zip Code")]
        public string? BillingZipCode { get; set; }

        [Display(Name = "Country")]
        public string? BillingCountry { get; set; }
    }

    // Order Stats View Model
    public class OrderStatsViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingPaymentOrders { get; set; }
        public int RefundedOrders { get; set; }
        public int InProgressOrders => ConfirmedOrders + ProcessingOrders + ShippedOrders;
    }

    // Admin Order Index View Model
    public class AdminOrderIndexViewModel
    {
        public List<Yash_Gems___Jewelleries.Models.Order> Orders { get; set; } = new();
        public OrderStatsViewModel Stats { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
        public string CurrentStatus { get; set; } = "All";
    }

    // Customer Order Index View Model
    public class CustomerOrderIndexViewModel
    {
        public List<Yash_Gems___Jewelleries.Models.Order> Orders { get; set; } = new();
    }

    // Pagination Metadata View Model
    public class PaginationMetadata
    {
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    // Order Detail View Model for Admin
    public class OrderDetailViewModel
    {
        public Yash_Gems___Jewelleries.Models.Order? Order { get; set; }
        public List<OrderTimelineStep> Timeline { get; set; } = new();
    }

    public class OrderTimelineStep
    {
        public OrderStatus Status { get; set; }
        public string? Label { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? Description { get; set; }
    }
}

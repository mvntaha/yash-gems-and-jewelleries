using System.ComponentModel.DataAnnotations;
using Yash_Gems___Jewelleries.Models;

namespace Yash_Gems___Jewelleries.ViewModels
{
    // Summary view model for customer
    public class CustomerSummaryViewModel
    {
        public ApplicationUser Customer { get; set; } = null!;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public string? LatestInvoiceId { get; set; }
        public DateTime? LatestOrderDate { get; set; }
        public string? PreferredPaymentMethod { get; set; }
    }

    // Detail view model for customer
    public class CustomerDetailViewModel
    {
        public ApplicationUser Customer { get; set; } = null!;
        public List<Order> Orders { get; set; } = new List<Order>();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalInvoices { get; set; }
        public Order? LatestOrder { get; set; }
        public bool IsNewsletterSubscribed { get; set; }
    }

    // Index view model for customer
    public class CustomerIndexViewModel
    {
        public List<CustomerSummaryViewModel> Customers { get; set; } = new List<CustomerSummaryViewModel>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        
        // Stats for cards
        public int AllCustomersCount { get; set; }
        public int TotalOrdersCount { get; set; }
        public int ServiceRequestsCount { get; set; } = 1030; 
        public decimal TotalRevenue { get; set; }
    }
}



namespace Yash_Gems___Jewelleries.Areas.Admin.Models.ViewModels.Report
{
    public class CustomerReportViewModel
    {

        public List<CustomerItemDTO> Items { get; set; } = new List<CustomerItemDTO>();
        public CustomerSummaryDTO Summary { get; set; } = new CustomerSummaryDTO();
    }

    public class CustomerItemDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }
        public string? LatestInvoiceId { get; set; }
        public string? PreferredPaymentMethod { get; set; }
        public DateTime RegisteredDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class CustomerSummaryDTO
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; } // With at least 1 order
        public decimal TotalRevenueFromCustomers { get; set; }
    }
}

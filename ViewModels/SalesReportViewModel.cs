using Yash_Gems___Jewelleries.Models.Enums;


namespace Yash_Gems___Jewelleries.Areas.Admin.Models.ViewModels.Report
{
    public class SalesReportViewModel
    {

        public List<SalesItemDTO> Items { get; set; } = new List<SalesItemDTO>();
        public SalesSummaryDTO Summary { get; set; } = new SalesSummaryDTO();
    }

    public class SalesItemDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
    }

    public class SalesSummaryDTO
    {
        public decimal TotalSales { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetSales { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}

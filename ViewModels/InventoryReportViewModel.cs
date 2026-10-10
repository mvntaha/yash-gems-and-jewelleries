namespace Yash_Gems___Jewelleries.Areas.Admin.Models.ViewModels.Report
{
    public class InventoryReportViewModel
    {

        public List<InventoryItemDTO> Items { get; set; } = new List<InventoryItemDTO>();
        public InventorySummaryDTO Summary { get; set; } = new InventorySummaryDTO();
    }

    public class InventoryItemDTO
    {
        public string StyleCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal MRP { get; set; }
        public decimal TotalValue { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty; // In Stock, Low Stock, Out of Stock
    }

    public class InventorySummaryDTO
    {
        public int TotalItems { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}

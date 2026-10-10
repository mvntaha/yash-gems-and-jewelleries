using Yash_Gems___Jewelleries.Areas.Admin.Models.ViewModels.Report;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface IReportService
    {
        Task<SalesReportViewModel> GetSalesReportAsync();
        Task<InventoryReportViewModel> GetInventoryReportAsync();
        Task<CustomerReportViewModel> GetCustomerReportAsync();
        
        Task<byte[]> ExportSalesReportToExcelAsync(SalesReportViewModel data);
        Task<byte[]> ExportInventoryReportToExcelAsync(InventoryReportViewModel data);
        Task<byte[]> ExportCustomerReportToExcelAsync(CustomerReportViewModel data);

        Task<byte[]> ExportSalesReportToPdfAsync(SalesReportViewModel data);
        Task<byte[]> ExportInventoryReportToPdfAsync(InventoryReportViewModel data);
        Task<byte[]> ExportCustomerReportToPdfAsync(CustomerReportViewModel data);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> Sales()
        {
            var model = await _reportService.GetSalesReportAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSales()
        {
            var model = await _reportService.GetSalesReportAsync();
            var content = await _reportService.ExportSalesReportToExcelAsync(model);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SalesReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            var model = await _reportService.GetInventoryReportAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportInventory()
        {
            var model = await _reportService.GetInventoryReportAsync();
            var content = await _reportService.ExportInventoryReportToExcelAsync(model);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        
        [HttpGet]
        public async Task<IActionResult> Customer()
        {
            var model = await _reportService.GetCustomerReportAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomer()
        {
            var model = await _reportService.GetCustomerReportAsync();
            var content = await _reportService.ExportCustomerReportToExcelAsync(model);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CustomerReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportSalesPdf()
        {
            var model = await _reportService.GetSalesReportAsync();
            var content = await _reportService.ExportSalesReportToPdfAsync(model);
            return File(content, "application/pdf", $"SalesReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> ExportInventoryPdf()
        {
            var model = await _reportService.GetInventoryReportAsync();
            var content = await _reportService.ExportInventoryReportToPdfAsync(model);
            return File(content, "application/pdf", $"InventoryReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> ExportCustomerPdf()
        {
            var model = await _reportService.GetCustomerReportAsync();
            var content = await _reportService.ExportCustomerReportToPdfAsync(model);
            return File(content, "application/pdf", $"CustomerReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
    }
}

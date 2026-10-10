using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Interfaces;
using Yash_Gems___Jewelleries.Models.Enums;
using Yash_Gems___Jewelleries.Areas.Admin.Models.ViewModels.Report;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Yash_Gems___Jewelleries.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReportViewModel> GetSalesReportAsync()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Select(o => new SalesItemDTO
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber ?? string.Empty,
                    OrderDate = o.OrderDate,
                    CustomerName = o.CustomerName ?? string.Empty,
                    Subtotal = o.Subtotal,
                    TaxAmount = o.TaxAmount,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod ?? string.Empty,
                    PaymentStatus = o.PaymentStatus ?? string.Empty,
                    OrderStatus = o.OrderStatus.ToString()
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var summary = new SalesSummaryDTO
            {
                TotalSales = orders.Sum(o => o.TotalAmount),
                TotalTax = orders.Sum(o => o.TaxAmount),
                TotalDiscount = orders.Sum(o => o.DiscountAmount),
                NetSales = orders.Sum(o => o.Subtotal),
                TotalOrders = orders.Count,
                CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered.ToString()),
                PendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending.ToString() || o.OrderStatus == OrderStatus.Processing.ToString()),
                CancelledOrders = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled.ToString())
            };

            return new SalesReportViewModel
            {
                Items = orders,
                Summary = summary
            };
        }

        public async Task<byte[]> ExportSalesReportToExcelAsync(SalesReportViewModel data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sales Report");

            // Headers
            worksheet.Cell(1, 1).Value = "Order #";
            worksheet.Cell(1, 2).Value = "Date";
            worksheet.Cell(1, 3).Value = "Customer";
            worksheet.Cell(1, 4).Value = "Payment";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Total";

            // Data
            int row = 2;
            foreach (var item in data.Items)
            {
                worksheet.Cell(row, 1).Value = item.OrderNumber;
                worksheet.Cell(row, 2).Value = item.OrderDate;
                worksheet.Cell(row, 3).Value = item.CustomerName;
                worksheet.Cell(row, 4).Value = item.PaymentMethod;
                worksheet.Cell(row, 5).Value = item.OrderStatus;
                worksheet.Cell(row, 6).Value = item.TotalAmount;
                row++;
            }
            
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<InventoryReportViewModel> GetInventoryReportAsync()
        {
            var items = await _context.Items
                .AsNoTracking()
                .Select(i => new InventoryItemDTO
                {
                    StyleCode = i.StyleCode ?? string.Empty,
                    ItemName = i.ItemName ?? string.Empty,
                    CategoryName = i.Category != null ? i.Category.CategoryName : "Unknown",
                    BrandName = i.Brand != null ? i.Brand.BrandType : "Unknown",
                    Quantity = i.Quantity,
                    MRP = i.MRP,
                    TotalValue = i.Quantity * i.MRP,
                    IsActive = i.IsActive
                })
                .ToListAsync();

            // Calculate status client-side
            foreach(var item in items)
            {
                if (item.Quantity == 0) item.Status = "Out of Stock";
                else if (item.Quantity < 5) item.Status = "Low Stock";
                else item.Status = "In Stock";
            }

            var summary = new InventorySummaryDTO
            {
                TotalItems = items.Count,
                TotalStock = items.Sum(i => i.Quantity),
                TotalInventoryValue = items.Sum(i => i.TotalValue),
                LowStockCount = items.Count(i => i.Quantity < 5 && i.Quantity > 0),
                OutOfStockCount = items.Count(i => i.Quantity == 0)
            };

            return new InventoryReportViewModel
            {
                Items = items,
                Summary = summary
            };
        }

        public async Task<byte[]> ExportInventoryReportToExcelAsync(InventoryReportViewModel data)
        {
             using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Report");

            worksheet.Cell(1, 1).Value = "Style Code";
            worksheet.Cell(1, 2).Value = "Item Name";
            worksheet.Cell(1, 3).Value = "Category";
            worksheet.Cell(1, 4).Value = "Brand";
            worksheet.Cell(1, 5).Value = "Stock";
            worksheet.Cell(1, 6).Value = "MRP";
            worksheet.Cell(1, 7).Value = "Total Value";
            worksheet.Cell(1, 8).Value = "Status";

            int row = 2;
            foreach (var item in data.Items)
            {
                worksheet.Cell(row, 1).Value = item.StyleCode;
                worksheet.Cell(row, 2).Value = item.ItemName;
                worksheet.Cell(row, 3).Value = item.CategoryName;
                worksheet.Cell(row, 4).Value = item.BrandName;
                worksheet.Cell(row, 5).Value = item.Quantity;
                worksheet.Cell(row, 6).Value = item.MRP;
                worksheet.Cell(row, 7).Value = item.TotalValue;
                worksheet.Cell(row, 8).Value = item.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<CustomerReportViewModel> GetCustomerReportAsync()
        {
            var customerRole = await _context.Roles
                .Where(r => r.Name == "Customer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var query = _context.Users.AsNoTracking().AsQueryable();

            if (customerRole != null)
            {
                query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == customerRole));
            }

            var users = await query
                .Select(u => new 
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.ProfilePictureUrl,
                    u.IsActive,
                    OrderCount = u.Orders.Count,
                    TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                    LastOrder = u.Orders.OrderByDescending(o => o.OrderDate).Select(o => new { o.OrderDate, o.OrderNumber, o.PaymentMethod }).FirstOrDefault()
                })
                .ToListAsync();

            var customerDtos = users.Select(u => new CustomerItemDTO 
            {
                UserId = u.Id,
                FullName = !string.IsNullOrEmpty(u.FirstName) ? $"{u.FirstName} {u.LastName}" : (u.UserName ?? "Unknown"),
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ProfilePictureUrl = u.ProfilePictureUrl,
                IsActive = u.IsActive,
                LatestInvoiceId = u.LastOrder?.OrderNumber,
                PreferredPaymentMethod = u.LastOrder?.PaymentMethod,
                TotalOrders = u.OrderCount,
                TotalSpent = u.TotalSpent,
                LastOrderDate = u.LastOrder?.OrderDate
            }).ToList();

            var summary = new CustomerSummaryDTO
            {
                TotalCustomers = customerDtos.Count,
                ActiveCustomers = customerDtos.Count(c => c.TotalOrders > 0),
                TotalRevenueFromCustomers = customerDtos.Sum(c => c.TotalSpent)
            };

            return new CustomerReportViewModel
            {
                Items = customerDtos,
                Summary = summary
            };
        }

        public async Task<byte[]> ExportCustomerReportToExcelAsync(CustomerReportViewModel data)
        {
             using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Customer Report");

            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Orders";
            worksheet.Cell(1, 4).Value = "Total Spent";
            worksheet.Cell(1, 5).Value = "Last Purchase";

            int row = 2;
            foreach (var item in data.Items)
            {
                worksheet.Cell(row, 1).Value = item.FullName;
                worksheet.Cell(row, 2).Value = item.Email;
                worksheet.Cell(row, 3).Value = item.TotalOrders;
                worksheet.Cell(row, 4).Value = item.TotalSpent;
                worksheet.Cell(row, 5).Value = item.LastOrderDate;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportSalesReportToPdfAsync(SalesReportViewModel data)
        {
            // Simple PDF generation using QuestPDF
            // Ensure License is set in Program.cs: QuestPDF.Settings.License = LicenseType.Community;
            
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Sales Report").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                            column.Item().Text($"Generated on: {DateTime.Now:g}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Summary Section
                        column.Item().Table(summaryTable =>
                        {
                            summaryTable.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            summaryTable.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Sales").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalSales.ToString("C")).FontSize(12).SemiBold();
                            });
                            summaryTable.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Net Sales").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.NetSales.ToString("C")).FontSize(12).SemiBold();
                            });
                            summaryTable.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Orders").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalOrders.ToString()).FontSize(12).SemiBold();
                            });
                            summaryTable.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Pending Sales").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.PendingOrders.ToString()).FontSize(12).SemiBold();
                            });
                        });

                        // Data Table
                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(70);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Order #");
                                header.Cell().Element(CellStyle).Text("Date");
                                header.Cell().Element(CellStyle).Text("Customer");
                                header.Cell().Element(CellStyle).Text("Status");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                                }
                            });

                            foreach (var item in data.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.OrderNumber);
                                table.Cell().Element(CellStyle).Text(item.OrderDate.ToShortDateString());
                                table.Cell().Element(CellStyle).Text(item.CustomerName);
                                table.Cell().Element(CellStyle).Text(item.OrderStatus.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.TotalAmount.ToString("C"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportInventoryReportToPdfAsync(InventoryReportViewModel data)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Inventory Report").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Green.Medium);
                            column.Item().Text($"Generated on: {DateTime.Now:g}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                    });
                    
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Summary Section
                        column.Item().PaddingBottom(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Items").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalItems.ToString()).FontSize(12).SemiBold();
                            });
                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Value").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalInventoryValue.ToString("C")).FontSize(12).SemiBold();
                            });
                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Low Stock").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.LowStockCount.ToString()).FontSize(12).SemiBold();
                            });
                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Out of Stock").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.OutOfStockCount.ToString()).FontSize(12).SemiBold();
                            });
                        });
                        
                        // Data Table
                        column.Item().PaddingTop(10).Table(table =>
                        {
                             table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Style Code");
                                header.Cell().Element(CellStyle).Text("Item Name");
                                header.Cell().Element(CellStyle).Text("Category");
                                header.Cell().Element(CellStyle).Text("Stock");
                                header.Cell().Element(CellStyle).Text("Status");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                                }
                            });
                            
                            foreach (var item in data.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.StyleCode);
                                table.Cell().Element(CellStyle).Text(item.ItemName);
                                table.Cell().Element(CellStyle).Text(item.CategoryName);
                                table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).Text(item.Status);

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });
                    });

                     page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });
            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportCustomerReportToPdfAsync(CustomerReportViewModel data)
        {
             var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Customer Report").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Orange.Medium);
                            column.Item().Text($"Generated on: {DateTime.Now:g}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                    });
                    
                     page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Summary Section
                        column.Item().PaddingBottom(10).Table(table =>
                        {
                             table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Customers").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalCustomers.ToString()).FontSize(12).SemiBold();
                            });
                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Active Customers").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.ActiveCustomers.ToString()).FontSize(12).SemiBold();
                            });
                             table.Cell().Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Revenue").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                c.Item().Text(data.Summary.TotalRevenueFromCustomers.ToString("C")).FontSize(12).SemiBold();
                            });
                        });
                        
                        // Data Table
                        column.Item().PaddingTop(10).Table(table =>
                        {
                             table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Name");
                                header.Cell().Element(CellStyle).Text("Email");
                                header.Cell().Element(CellStyle).Text("Orders");
                                header.Cell().Element(CellStyle).Text("Spent");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                                }
                            });
                            
                            foreach (var item in data.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.FullName);
                                table.Cell().Element(CellStyle).Text(item.Email);
                                table.Cell().Element(CellStyle).Text(item.TotalOrders.ToString());
                                table.Cell().Element(CellStyle).Text(item.TotalSpent.ToString("C"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}

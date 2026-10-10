using System;
using System.Collections.Generic;

namespace Yash_Gems___Jewelleries.ViewModels
{
    public class DashboardViewModel
    {
        // KPI: Total Orders
        public int TotalOrders { get; set; }
        public double OrdersGrowth { get; set; } // Percentage growth compared to last period

        // KPI: Total Customers
        public int TotalCustomers { get; set; }
        public double CustomersGrowth { get; set; }

        // KPI: Total Products
        public int TotalProducts { get; set; }
        public double ProductsGrowth { get; set; }

        // KPI: Total Revenue
        public decimal TotalRevenue { get; set; }
        public double RevenueGrowth { get; set; }

        // Top Customers List
        public List<CustomerSummary> TopCustomers { get; set; } = new List<CustomerSummary>();

        // Top Products List
        public List<TopProductSummary> TopProducts { get; set; } = new List<TopProductSummary>();

        // Recent Orders List
        public List<OrderSummary> RecentOrders { get; set; } = new List<OrderSummary>();
    }

    public class TopProductSummary
    {
        public string StyleCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CustomerSummary
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public double SpendingPercentage { get; set; } // Compared to total revenue
        public string AvatarUrl { get; set; } = string.Empty; // Optional, if we assume avatars
    }

    public class OrderSummary
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty; // City/State
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ProductImageUrl { get; set; } = string.Empty; // First product image
    }

    // Chart Data Classes
    public class SalesChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Revenue { get; set; } = new List<decimal>();
        public List<int> Orders { get; set; } = new List<int>();
    }

    public class ConversionChartData
    {
        public double ConversionRate { get; set; }
        public int ThisWeekVisits { get; set; } // Placeholder if we track visits
        public int ThisWeekOrders { get; set; }
        public int LastWeekVisits { get; set; }
        public int LastWeekOrders { get; set; }
    }
}

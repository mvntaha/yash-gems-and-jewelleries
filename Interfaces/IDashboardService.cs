using System.Threading.Tasks;
using Yash_Gems___Jewelleries.ViewModels;

namespace Yash_Gems___Jewelleries.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the main dashboard summary including KPIs, recent orders, and top customers.
        /// </summary>
        /// <returns>DashboardViewModel populated with data.</returns>
        Task<DashboardViewModel> GetDashboardSummaryAsync();

        /// <summary>
        /// Retrieves data for the sales chart based on the specified period.
        /// </summary>
        /// <param name="period">The period to filter by (e.g., "1M", "6M", "1Y", "ALL").</param>
        /// <returns>SalesChartData object.</returns>
        Task<SalesChartData> GetSalesChartDataAsync(string period);

        /// <summary>
        /// Retrieves data for the conversion chart.
        /// </summary>
        /// <returns>ConversionChartData object.</returns>
        Task<ConversionChartData> GetOrderConversionDataAsync();

        /// <summary>
        /// Retrieves the top 5 products based on total revenue.
        /// </summary>
        /// <returns>List of TopProductSummary.</returns>
        Task<List<TopProductSummary>> GetTopProductsAsync(int count = 5);

        /// <summary>
        /// Retrieves daily revenue statistics for a given period.
        /// </summary>
        /// <param name="days">Number of days to look back.</param>
        /// <returns>List of DailyRevenueDto.</returns>
        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(int days = 30);
    }
}

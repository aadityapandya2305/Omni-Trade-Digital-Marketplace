namespace OmniTradeWebApi.DTOs
{
    public class PlatformAnalyticsDto
    {
        // Original stats
        public decimal GMV { get; set; }

        public int TotalActiveVendors { get; set; }

        public int TotalOrders { get; set; }

        // Quick-win stats
        public int TotalUsers { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalVendorUsers { get; set; }

        public int TotalAdmins { get; set; }

        public int PendingVendorApprovals { get; set; }

        public int TotalProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        public decimal AverageOrderValue { get; set; }

        public int NewUsersLast7Days { get; set; }

        // Grouped breakdowns
        public IEnumerable<StatusCountDto> OrdersByStatus { get; set; }
            = new List<StatusCountDto>();

        public IEnumerable<CategoryRevenueDto> RevenueByCategory { get; set; }
            = new List<CategoryRevenueDto>();

        public IEnumerable<DailyRevenueDto> RevenueTrend { get; set; }
            = new List<DailyRevenueDto>();

        public IEnumerable<VendorRevenueDto> TopVendors { get; set; }
            = new List<VendorRevenueDto>();

        public IEnumerable<ProductSalesDto> TopProducts { get; set; }
            = new List<ProductSalesDto>();
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = null!;

        public int Count { get; set; }
    }

    public class CategoryRevenueDto
    {
        public string Category { get; set; } = null!;

        public decimal Revenue { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }

        public decimal Revenue { get; set; }
    }

    public class VendorRevenueDto
    {
        public int VendorId { get; set; }

        public string StoreName { get; set; } = null!;

        public decimal Revenue { get; set; }
    }

    public class ProductSalesDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int UnitsSold { get; set; }
    }
}
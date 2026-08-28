namespace OmniTradeMvc.Models
{
    public class PlatformAnalyticsViewModel
    {
        public decimal GMV { get; set; }

        public int TotalActiveVendors { get; set; }

        public int TotalOrders { get; set; }

        public int TotalUsers { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalVendorUsers { get; set; }

        public int TotalAdmins { get; set; }

        public int PendingVendorApprovals { get; set; }

        public int TotalProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        public decimal AverageOrderValue { get; set; }

        public int NewUsersLast7Days { get; set; }

        public List<StatusCountViewModel> OrdersByStatus { get; set; } = new();

        public List<CategoryRevenueViewModel> RevenueByCategory { get; set; } = new();

        public List<DailyRevenueViewModel> RevenueTrend { get; set; } = new();

        public List<VendorRevenueViewModel> TopVendors { get; set; } = new();

        public List<ProductSalesViewModel> TopProducts { get; set; } = new();
    }

    public class StatusCountViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CategoryRevenueViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class VendorRevenueViewModel
    {
        public int VendorId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class ProductSalesViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
    }
}
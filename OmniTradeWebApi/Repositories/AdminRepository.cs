using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.DTOs;

namespace OmniTradeWebApi.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly OmniTradeHubContext _context;

        public AdminRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync()
        {
            var gmv = await _context.OrderItems
                .SumAsync(oi => oi.Quantity * oi.UnitPrice);

            var totalActiveVendors = await _context.Vendors
                .CountAsync(v => v.IsApproved == true);

            var totalOrders = await _context.Orders
                .CountAsync();

            var totalUsers = await _context.Users.CountAsync();

            var totalCustomers = await _context.Users
                .CountAsync(u => u.Role == "Customer");

            var totalVendorUsers = await _context.Users
                .CountAsync(u => u.Role == "Vendor");

            var totalAdmins = await _context.Users
                .CountAsync(u => u.Role == "Admin");

            var pendingVendorApprovals = await _context.Vendors
                .CountAsync(v => v.IsApproved != true);

            var totalProducts = await _context.Products.CountAsync();

            var outOfStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity == 0);

            var averageOrderValue = totalOrders == 0
                ? 0
                : await _context.Orders.AverageAsync(o => o.TotalAmount);

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var newUsersLast7Days = await _context.Users
                .CountAsync(u => u.CreatedAt != null && u.CreatedAt >= sevenDaysAgo);

            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var revenueByCategory = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Product.Category)
                .Select(g => new CategoryRevenueDto
                {
                    Category = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(c => c.Revenue)
                .ToListAsync();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var revenueTrend = await _context.Orders
                .Where(o => o.OrderDate != null && o.OrderDate >= thirtyDaysAgo)
                .GroupBy(o => o.OrderDate!.Value.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            var topVendors = await _context.OrderItems
                .Include(oi => oi.Vendor)
                .GroupBy(oi => new { oi.VendorId, oi.Vendor.StoreName })
                .Select(g => new VendorRevenueDto
                {
                    VendorId = g.Key.VendorId,
                    StoreName = g.Key.StoreName,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(v => v.Revenue)
                .Take(5)
                .ToListAsync();

            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new ProductSalesDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(p => p.UnitsSold)
                .Take(5)
                .ToListAsync();

            return new PlatformAnalyticsDto
            {
                GMV = gmv,
                TotalActiveVendors = totalActiveVendors,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalVendorUsers = totalVendorUsers,
                TotalAdmins = totalAdmins,
                PendingVendorApprovals = pendingVendorApprovals,
                TotalProducts = totalProducts,
                OutOfStockProducts = outOfStockProducts,
                AverageOrderValue = averageOrderValue,
                NewUsersLast7Days = newUsersLast7Days,
                OrdersByStatus = ordersByStatus,
                RevenueByCategory = revenueByCategory,
                RevenueTrend = revenueTrend,
                TopVendors = topVendors,
                TopProducts = topProducts
            };
        }

        public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserManagementDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VendorManagementDto>> GetAllVendorsAsync()
        {
            return await _context.Vendors
                .Select(v => new VendorManagementDto
                {
                    Id = v.Id,
                    UserId = v.UserId,
                    StoreName = v.StoreName,
                    ContactEmail = v.ContactEmail,
                    IsApproved = v.IsApproved
                })
                .ToListAsync();
        }
    }
}
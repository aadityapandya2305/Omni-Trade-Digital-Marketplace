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

            return new PlatformAnalyticsDto
            {
                GMV = gmv,
                TotalActiveVendors = totalActiveVendors,
                TotalOrders = totalOrders
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
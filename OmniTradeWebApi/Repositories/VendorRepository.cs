using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public class VendorRepository : IVendorRepository
    {
        private readonly OmniTradeHubContext _context;

        public VendorRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<Vendor?> GetVendorByIdAsync(int id)
        {
            return await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vendor?> GetVendorByUserIdAsync(int userId)
        {
            return await _context.Vendors
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task RegisterVendorProfileAsync(Vendor vendor)
        {
            await _context.Vendors.AddAsync(vendor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVendorApprovalAsync(
            int vendorId,
            bool isApproved)
        {
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor != null)
            {
                vendor.IsApproved = isApproved;
                await _context.SaveChangesAsync();
            }
        }
    }
}
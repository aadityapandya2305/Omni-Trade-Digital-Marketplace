using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IVendorRepository
    {
        Task<Vendor?> GetVendorByIdAsync(int id);

        Task<Vendor?> GetVendorByUserIdAsync(int userId);

        Task RegisterVendorProfileAsync(Vendor vendor);

        Task UpdateVendorApprovalAsync(int vendorId, bool isApproved);

    }
}
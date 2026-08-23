using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public interface IAdminService
    {
        Task<PlatformAnalyticsViewModel?> GetPlatformAnalyticsAsync();

        Task<IEnumerable<UserManagementViewModel>> GetAllUsersAsync();

        Task<IEnumerable<VendorManagementViewModel>> GetAllVendorsAsync();

        Task<bool> UpdateVendorApprovalAsync(int vendorId, bool isApproved);
    }
}
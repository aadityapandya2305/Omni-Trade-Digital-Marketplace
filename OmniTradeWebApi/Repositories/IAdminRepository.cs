using OmniTradeWebApi.DTOs;

namespace OmniTradeWebApi.Repositories
{
    public interface IAdminRepository
    {
        Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync();

        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();

        Task<IEnumerable<VendorManagementDto>> GetAllVendorsAsync();
    }
}
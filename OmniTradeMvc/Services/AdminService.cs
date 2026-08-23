using System.Net.Http.Headers;
using System.Net.Http.Json;
using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public class AdminService : IAdminService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient GetClient()
        {
            var client =
                _httpClientFactory.CreateClient("OmniTradeApi");

            var token =
                _httpContextAccessor.HttpContext?
                    .Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }

            return client;
        }

        public async Task<PlatformAnalyticsViewModel?>
            GetPlatformAnalyticsAsync()
        {
            var client = GetClient();

            return await client.GetFromJsonAsync
                <PlatformAnalyticsViewModel>(
                    "api/admin/analytics");
        }

        public async Task<IEnumerable<UserManagementViewModel>>
            GetAllUsersAsync()
        {
            var client = GetClient();

            return await client.GetFromJsonAsync
                <IEnumerable<UserManagementViewModel>>(
                    "api/admin/users")
                ?? Enumerable.Empty<UserManagementViewModel>();
        }

        public async Task<IEnumerable<VendorManagementViewModel>>
            GetAllVendorsAsync()
        {
            var client = GetClient();

            return await client.GetFromJsonAsync
                <IEnumerable<VendorManagementViewModel>>(
                    "api/admin/vendors")
                ?? Enumerable.Empty<VendorManagementViewModel>();
        }

        public async Task<bool> UpdateVendorApprovalAsync(
            int vendorId,
            bool isApproved)
        {
            var client = GetClient();

            var response = await client.PatchAsJsonAsync(
                $"api/vendors/{vendorId}/approve",
                isApproved);

            return response.IsSuccessStatusCode;
        }
    }
}
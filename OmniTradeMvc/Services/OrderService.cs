using System.Net.Http.Headers;
using System.Net.Http.Json;
using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public class OrderService : IOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
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

        public async Task<IEnumerable<VendorOrderItemViewModel>>
            GetIncomingOrdersAsync(int vendorId)
        {
            var client = GetClient();

            return await client.GetFromJsonAsync
                <IEnumerable<VendorOrderItemViewModel>>(
                    $"api/Orders/vendor/{vendorId}")
                ?? Enumerable.Empty<VendorOrderItemViewModel>();
        }

        public async Task<VendorOrderDetailsViewModel?>
            GetOrderDetailsAsync(
                int vendorId,
                int orderId)
        {
            var client = GetClient();

            return await client.GetFromJsonAsync
                <VendorOrderDetailsViewModel>(
                    $"api/Orders/vendor/{vendorId}/{orderId}");
        }

        public async Task<bool> UpdateOrderStatusAsync(
            int orderId,
            string status)
        {
            var client = GetClient();

            var response = await client.PatchAsJsonAsync(
                $"api/Orders/{orderId}/status",
                status);

            return response.IsSuccessStatusCode;
        }

        public async Task<int?> GetCurrentVendorIdAsync()
        {
            var client = GetClient();

            var dashboard = await client.GetFromJsonAsync<VendorDashboardViewModel>(
                "api/Vendors/dashboard");

            return dashboard?.VendorId;
        }
    }
}
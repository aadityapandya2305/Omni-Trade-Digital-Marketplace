using System.Net.Http.Headers;
using System.Net.Http.Json;
using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public class VendorService : IVendorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VendorService(
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

        public async Task<int?> GetCurrentVendorIdAsync()
        {
            var client = GetClient();

            var dashboard = await client.GetFromJsonAsync
                <VendorDashboardViewModel>(
                    "api/Vendors/dashboard");

            return dashboard?.VendorId;
        }

        public async Task<VendorDashboardViewModel?> GetDashboardAsync()
        {
            var client = GetClient();

            var response = await client.GetAsync("api/Vendors/dashboard");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<VendorDashboardViewModel>();
        }

        public async Task<VendorProfileViewModel?> GetProfileAsync()
        {
            var client = GetClient();

            var vendorId = await GetCurrentVendorIdAsync();

            if (vendorId == null)
            {
                return null;
            }

            var response = await client.GetAsync($"api/Vendors/{vendorId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // The WebApi Vendor DTO uses "Id" (not "VendorId"), so map the
            // fields explicitly rather than relying on a direct match.
            var vendor = await response.Content
                .ReadFromJsonAsync<VendorApiModel>();

            if (vendor == null)
            {
                return null;
            }

            return new VendorProfileViewModel
            {
                VendorId = vendor.Id,
                StoreName = vendor.StoreName,
                Description = vendor.Description,
                ContactEmail = vendor.ContactEmail,
                IsApproved = vendor.IsApproved
            };
        }

        // Mirrors the shape of OmniTradeWebApi.Models.Vendor for deserialization.
        private class VendorApiModel
        {
            public int Id { get; set; }
            public string StoreName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string ContactEmail { get; set; } = string.Empty;
            public bool? IsApproved { get; set; }
        }

        public async Task<bool> UpdateProfileAsync(VendorProfileViewModel model)
        {
            var client = GetClient();

            var response = await client.PutAsJsonAsync(
                "api/Vendors/profile",
                new
                {
                    storeName = model.StoreName,
                    description = model.Description,
                    contactEmail = model.ContactEmail
                });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterVendorAsync(VendorProfileViewModel model)
        {
            var client = GetClient();

            var response = await client.PostAsJsonAsync(
                "api/Vendors/register",
                new
                {
                    storeName = model.StoreName,
                    description = model.Description,
                    contactEmail = model.ContactEmail
                });

            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<ProductViewModel>> GetMyProductsAsync()
        {
            var client = GetClient();

            var vendorId = await GetCurrentVendorIdAsync();

            if (vendorId == null)
            {
                return Enumerable.Empty<ProductViewModel>();
            }

            return await client.GetFromJsonAsync
                <IEnumerable<ProductViewModel>>(
                    $"api/Products/filter?vendorId={vendorId}")
                ?? Enumerable.Empty<ProductViewModel>();
        }

        public async Task<ProductViewModel?> GetMyProductAsync(int productId)
        {
            var client = GetClient();

            var vendorId = await GetCurrentVendorIdAsync();

            if (vendorId == null)
            {
                return null;
            }

            var product = await client.GetFromJsonAsync<ProductViewModel>(
                $"api/Products/{productId}");

            if (product == null || product.VendorId != vendorId)
            {
                return null;
            }

            return product;
        }

        public async Task<bool> CreateProductAsync(ProductViewModel model)
        {
            var client = GetClient();

            var response = await client.PostAsJsonAsync(
                "api/Products",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductAsync(
            int productId,
            ProductViewModel model)
        {
            var client = GetClient();

            var response = await client.PutAsJsonAsync(
                $"api/Products/{productId}",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var client = GetClient();

            var response = await client.DeleteAsync(
                $"api/Products/{productId}");

            return response.IsSuccessStatusCode;
        }
    }
}
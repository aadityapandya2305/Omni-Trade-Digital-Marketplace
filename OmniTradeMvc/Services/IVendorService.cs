using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public interface IVendorService
    {
        Task<int?> GetCurrentVendorIdAsync();

        Task<VendorDashboardViewModel?> GetDashboardAsync();

        Task<VendorProfileViewModel?> GetProfileAsync();

        Task<bool> UpdateProfileAsync(VendorProfileViewModel model);

        Task<bool> RegisterVendorAsync(VendorProfileViewModel model);

        Task<IEnumerable<ProductViewModel>> GetMyProductsAsync();

        Task<ProductViewModel?> GetMyProductAsync(int productId);

        Task<bool> CreateProductAsync(ProductViewModel model);

        Task<bool> UpdateProductAsync(int productId, ProductViewModel model);

        Task<bool> DeleteProductAsync(int productId);
    }
}
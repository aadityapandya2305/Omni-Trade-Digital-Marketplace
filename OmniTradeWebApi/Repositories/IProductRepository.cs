using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();

        Task<Product?> GetProductByIdAsync(int id);

        Task<IEnumerable<Product>> GetProductsByNameAsync(string name);

        Task<IEnumerable<Product>> GetProductsByFilterAsync(string? name = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, int? vendorId = null);

        Task AddProductAsync(Product product);

        Task UpdateProductAsync(Product product);

        Task DeleteProductAsync(int id);
    }
}
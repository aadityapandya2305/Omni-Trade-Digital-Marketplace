using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();

        Task<Product?> GetProductByIdAsync(int id);

        Task<IEnumerable<Product>> GetProductsByNameAsync(string name);

        Task AddProductAsync(Product product);

        Task UpdateProductAsync(Product product);

        Task DeleteProductAsync(int id);
    }
}
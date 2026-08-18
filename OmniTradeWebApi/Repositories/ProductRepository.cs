using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OmniTradeHubContext _context;

        public ProductRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Vendor)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByNameAsync(string name)
        {
            return await _context.Products
                .Include(p => p.Vendor)
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
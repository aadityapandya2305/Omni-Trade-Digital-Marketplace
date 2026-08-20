using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OmniTradeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;

        public ProductsController(
            IProductRepository productRepository,
            IVendorRepository vendorRepository)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productRepository.GetAllProductsAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
    [FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search name cannot be empty.");
            }

            var products = await _productRepository
                .GetProductsByNameAsync(name);

            return Ok(products);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByFilter(
            [FromQuery] string? name = null,
            [FromQuery] string? category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? vendorId = null)
        {
            var products = await _productRepository
                .GetProductsByFilterAsync(name, category, minPrice, maxPrice, vendorId);

            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> AddProduct(Product product)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor = await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return BadRequest("Vendor profile does not exist.");
            }

            if (vendor.IsApproved != true)
            {
                return StatusCode(403, "Vendor is not approved.");
            }

            var newProduct = new Product
            {
                VendorId = vendor.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category
            };

            await _productRepository.AddProductAsync(newProduct);

            return StatusCode(201, new
            {
                message = "Product created successfully.",
                productId = newProduct.Id
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor = await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return BadRequest("Vendor profile does not exist.");
            }

            if (vendor.IsApproved != true)
            {
                return StatusCode(403, "Vendor is not approved.");
            }

            var existingProduct = await _productRepository.GetProductByIdAsync(id);

            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            if (existingProduct.VendorId != vendor.Id)
            {
                return StatusCode(403, "You can only update your own products.");
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.Category = product.Category;

            await _productRepository.UpdateProductAsync(existingProduct);

            return Ok(new
            {
                message = "Product updated successfully."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor = await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return BadRequest("Vendor profile does not exist.");
            }

            if (vendor.IsApproved != true)
            {
                return StatusCode(403, "Vendor is not approved.");
            }

            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (product.VendorId != vendor.Id)
            {
                return StatusCode(403, "You can only delete your own products.");
            }

            await _productRepository.DeleteProductAsync(id);

            return Ok(new
            {
                message = "Product deleted successfully."
            });
        }
    }
}
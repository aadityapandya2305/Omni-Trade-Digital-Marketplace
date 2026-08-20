using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;

namespace OmniTradeMvc.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            var products = new List<ProductViewModel>
            {
                new ProductViewModel
                {
                    Id = 1,
                    VendorId = 1,
                    Name = "Wireless Headphones",
                    Description = "Bluetooth wireless headphones",
                    Price = 2499.00m,
                    StockQuantity = 15,
                    Category = "Electronics"
                },
                new ProductViewModel
                {
                    Id = 2,
                    VendorId = 2,
                    Name = "Laptop Backpack",
                    Description = "Water-resistant laptop backpack",
                    Price = 1299.00m,
                    StockQuantity = 20,
                    Category = "Accessories"
                }
            };

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = new ProductViewModel
            {
                Id = id,
                VendorId = 1,
                Name = "Wireless Headphones",
                Description = "Bluetooth wireless headphones",
                Price = 2499.00m,
                StockQuantity = 15,
                Category = "Electronics"
            };

            return View(product);
        }
    }
}
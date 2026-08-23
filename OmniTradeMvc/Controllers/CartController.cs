using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int customerId = 1)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                var cartItems = await client.GetFromJsonAsync<List<CartApiItem>>(
                    $"api/Cart/{customerId}");

                var cart = new CartViewModel();

                if (cartItems != null)
                {
                    cart.Items = cartItems.Select(item => new CartItemViewModel
                    {
                        Id = item.Id,
                        CustomerId = item.CustomerId,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "Unknown Product",
                        Price = item.Product?.Price ?? 0,
                        Quantity = item.Quantity
                    }).ToList();
                }

                return View("Cart", cart);
            }
            catch (HttpRequestException)
            {
                return View("Cart", new CartViewModel());
            }
        }

        private class CartApiItem
        {
            public int Id { get; set; }

            public int CustomerId { get; set; }

            public int ProductId { get; set; }

            public int Quantity { get; set; }

            public CartApiProduct? Product { get; set; }
        }

        private class CartApiProduct
        {
            public string? Name { get; set; }

            public decimal Price { get; set; }
        }
    }
}
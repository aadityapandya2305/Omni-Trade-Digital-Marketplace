using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;
using OmniTradeMvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Customer")]
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (customerId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var cartItems = await client.GetFromJsonAsync<List<CartApiItem>>($"api/Cart/{customerId.Value}");

                var cart = new CartViewModel();

                if (cartItems != null)
                {
                    cart.Items = cartItems.Select(item => new CartItemViewModel
                    {
                        Id = item.Id,
                        CustomerId = item.CustomerId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity
                    }).ToList();
                }

                return View("Cart", cart);
            }
            catch (HttpRequestException)
            {
                TempData["CartError"] =
                    "Unable to connect to the cart service.";

                return View("Cart", new CartViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (customerId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new
                {
                    CustomerId = customerId.Value,
                    ProductId = productId,
                    Quantity = quantity
                };

                var response = await client.PostAsJsonAsync("api/Cart", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    TempData["CartError"] = $"Unable to add the product to your cart. API returned {(int)response.StatusCode}.";

                    return RedirectToAction("Details", "Products", new { id = productId });
                }

                TempData["CartMessage"] = "Product added to your cart.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["CartError"] = "Unable to connect to the cart service.";

                return RedirectToAction("Details", "Products", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (customerId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (quantity < 1)
            {
                TempData["CartError"] = "Quantity must be at least 1.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsJsonAsync($"api/Cart/{id}", quantity);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["CartError"] = "Unable to update the cart item.";
                }
                else
                {
                    TempData["CartMessage"] = "Cart quantity updated successfully.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["CartError"] = "Unable to connect to the cart service.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (customerId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"api/Cart/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["CartError"] = "Unable to remove the item from your cart.";
                }
                else
                {
                    TempData["CartMessage"] = "Item removed from your cart.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                TempData["CartError"] = "Unable to connect to the cart service.";

                return RedirectToAction(nameof(Index));
            }
        }

        private class CartApiItem
        {
            public int Id { get; set; }

            public int CustomerId { get; set; }

            public int ProductId { get; set; }

            public string ProductName { get; set; } = string.Empty;

            public decimal Price { get; set; }

            public int Quantity { get; set; }
        }
    }
}
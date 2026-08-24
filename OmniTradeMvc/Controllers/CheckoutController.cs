using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private HttpClient GetClient()
        {
            var client =
                _httpClientFactory.CreateClient("OmniTradeApi");

            var token =
                HttpContext.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // GET: /Checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = GetCurrentUserId();

            if (customerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = GetClient();

            try
            {
                var response =
                    await client.GetAsync(
                        $"api/Cart/{customerId.Value}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load your cart.";

                    return RedirectToAction(
                        "Index",
                        "Products");
                }

                var cartItems =
                    await response.Content
                        .ReadFromJsonAsync<List<CartApiItem>>();

                if (cartItems == null || cartItems.Count == 0)
                {
                    TempData["ErrorMessage"] =
                        "Your cart is empty.";

                    return RedirectToAction(
                        "Index",
                        "Products");
                }

                var cart = new CartViewModel
                {
                    Items = cartItems
                        .Select(item => new CartItemViewModel
                        {
                            Id = item.Id,
                            CustomerId = item.CustomerId,
                            ProductId = item.ProductId,
                            ProductName =
                                item.Product?.Name ?? "Unknown Product",
                            Price =
                                item.Product?.Price ?? 0,
                            Quantity = item.Quantity
                        })
                        .ToList()
                };

                var checkoutModel = new CheckoutViewModel
                {
                    Cart = cart
                };

                return View(checkoutModel);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the cart service.";

                return RedirectToAction(
                    "Index",
                    "Products");
            }
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            CheckoutViewModel model)
        {
            var customerId = GetCurrentUserId();

            if (customerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = GetClient();

            if (!ModelState.IsValid)
            {
                model.Cart =
                    await GetCartAsync(
                        client,
                        customerId.Value);

                return View("Index", model);
            }

            try
            {
                // Send shipping address and payment method
                // to the Web API.
                var checkoutRequest = new
                {
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod
                };

                // Web API endpoint:
                // POST api/Orders/checkout/{customerId}
                var response =
                    await client.PostAsJsonAsync(
                        $"api/Orders/checkout/{customerId.Value}",
                        checkoutRequest);

                if (response.IsSuccessStatusCode)
                {
                    var createdOrder =
                        await response.Content
                            .ReadFromJsonAsync<OrderViewModel>();

                    if (createdOrder != null)
                    {
                        TempData["SuccessMessage"] =
                            "Order placed successfully.";

                        return RedirectToAction(
                            nameof(Confirmation),
                            new { id = createdOrder.Id });
                    }

                    TempData["ErrorMessage"] =
                        "Order was created, but its details could not be loaded.";

                    return RedirectToAction(
                        "Index",
                        "Orders");
                }

                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Unable to place your order. Please try again."
                        : errorMessage);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the order service.");
            }

            // Reload cart if order creation failed
            model.Cart =
                await GetCartAsync(
                    client,
                    customerId.Value);

            return View("Index", model);
        }

        // GET: /Checkout/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var customerId = GetCurrentUserId();

            if (customerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = GetClient();

            try
            {
                var response =
                    await client.GetAsync(
                        $"api/Orders/customer/{customerId.Value}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(
                        "Index",
                        "Orders");
                }

                var apiOrder =
                    await response.Content
                        .ReadFromJsonAsync<OrderApiResponse>();

                if (apiOrder == null)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(
                        "Index",
                        "Orders");
                }

                var order = new OrderViewModel
                {
                    Id = apiOrder.Id,

                    CustomerId = apiOrder.CustomerId,

                    OrderDate =
                        apiOrder.OrderDate ?? DateTime.Now,

                    Status = apiOrder.Status,

                    ShippingAddress =
                        apiOrder.ShippingAddress ?? string.Empty,

                    PaymentMethod =
                        apiOrder.PaymentMethod ?? string.Empty,

                    TotalAmount = apiOrder.TotalAmount,

                    Items = apiOrder.OrderItems
                        .Select(item => new OrderItemViewModel
                        {
                            ProductId = item.ProductId,

                            ProductName =
                                item.Product?.Name
                                ?? "Unknown Product",

                            Price = item.UnitPrice,

                            Quantity = item.Quantity
                        })
                        .ToList()
                };

                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return RedirectToAction(
                    "Index",
                    "Orders");
            }
        }

        private async Task<CartViewModel> GetCartAsync(
            HttpClient client,
            int customerId)
        {
            var response =
                await client.GetAsync(
                    $"api/Cart/{customerId}");

            if (!response.IsSuccessStatusCode)
            {
                return new CartViewModel();
            }

            var cartItems =
                await response.Content
                    .ReadFromJsonAsync<List<CartApiItem>>()
                ?? new List<CartApiItem>();

            return new CartViewModel
            {
                Items = cartItems
                    .Select(item => new CartItemViewModel
                    {
                        Id = item.Id,
                        CustomerId = item.CustomerId,
                        ProductId = item.ProductId,
                        ProductName =
                            item.Product?.Name ?? "Unknown Product",
                        Price =
                            item.Product?.Price ?? 0,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };
        }

        // API cart response models

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

        // API order response models

        private class OrderApiResponse
        {
            public int Id { get; set; }

            public int CustomerId { get; set; }

            public DateTime? OrderDate { get; set; }

            public decimal TotalAmount { get; set; }

            public string Status { get; set; } = string.Empty;

            public string? ShippingAddress { get; set; }

            public string? PaymentMethod { get; set; }

            public List<OrderApiItem> OrderItems { get; set; } = new();
        }

        private class OrderApiItem
        {
            public int ProductId { get; set; }

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public OrderApiProduct? Product { get; set; }
        }

        private class OrderApiProduct
        {
            public string? Name { get; set; }

            public decimal Price { get; set; }
        }
    }
}
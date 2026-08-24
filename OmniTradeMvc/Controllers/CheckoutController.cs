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
        // Shows the cart summary before placing the order
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

                var cart =
                    await response.Content
                        .ReadFromJsonAsync<CartViewModel>();

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] =
                        "Your cart is empty.";

                    return RedirectToAction(
                        "Index",
                        "Products");
                }

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
        // Creates an order from the customer's current cart
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

            if (!ModelState.IsValid)
            {
                var clientForCart = GetClient();

                var cartResponse =
                    await clientForCart.GetAsync(
                        $"api/Cart/{customerId.Value}");

                if (cartResponse.IsSuccessStatusCode)
                {
                    model.Cart =
                        await cartResponse.Content
                            .ReadFromJsonAsync<CartViewModel>()
                        ?? new CartViewModel();
                }

                return View("Index", model);
            }

            var client = GetClient();

            try
            {
                // Web API creates the order from the customer's cart.
                // The API endpoint is:
                // POST api/Orders/checkout/{customerId}
                var response =
                    await client.PostAsync(
                        $"api/Orders/checkout/{customerId.Value}",
                        null);

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
                        nameof(Index),
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

            // Re-load cart if order creation failed
            var retryCartResponse =
                await client.GetAsync(
                    $"api/Cart/{customerId.Value}");

            if (retryCartResponse.IsSuccessStatusCode)
            {
                model.Cart =
                    await retryCartResponse.Content
                        .ReadFromJsonAsync<CartViewModel>()
                    ?? new CartViewModel();
            }

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
                // Use the customer-specific endpoint so the API
                // verifies that this order belongs to the logged-in user.
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

                var order =
                    await response.Content
                        .ReadFromJsonAsync<OrderViewModel>();

                if (order == null)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(
                        "Index",
                        "Orders");
                }

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
    }
}
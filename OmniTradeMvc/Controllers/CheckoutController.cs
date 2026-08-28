using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;
using OmniTradeMvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Customer")]
    public class CheckoutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = GetAuthorizedClient();

            try
            {
                var response = await client.GetAsync($"api/Cart/{customerId.Value}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to load your cart.";
                    return RedirectToAction("Index", "Products");
                }

                var items = await response.Content.ReadFromJsonAsync<List<CartItemViewModel>>() ?? new List<CartItemViewModel>();

                var cart = new CartViewModel { Items = items };

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Products");
                }

                var checkoutModel = new CheckoutViewModel
                {
                    Cart = cart
                };

                return View(checkoutModel);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the cart service.";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = GetAuthorizedClient();

            if (!ModelState.IsValid)
            {
                var cartResponse = await client.GetAsync($"api/Cart/{customerId.Value}");

                if (cartResponse.IsSuccessStatusCode)
                {
                    model.Cart = await cartResponse.Content.ReadFromJsonAsync<CartViewModel>()?? new CartViewModel();
                }

                return View("Index", model);
            }

            try
            {
                var response = await client.PostAsync($"api/Orders/checkout/{customerId.Value}",null);

                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>();
                    TempData["SuccessMessage"] = "Order placed successfully.";
                    return RedirectToAction(nameof(Confirmation), new { id = createdOrder?.Id });
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty,
                    string.IsNullOrWhiteSpace(errorBody) ? "Unable to place your order. Please try again." : errorBody);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the order service.");
            }

            var retryCartResponse = await client.GetAsync($"api/Cart/{customerId.Value}");
            if (retryCartResponse.IsSuccessStatusCode)
            {
                model.Cart = await retryCartResponse.Content.ReadFromJsonAsync<CartViewModel>() ?? new CartViewModel();
            }

            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var client = GetAuthorizedClient();

            try
            {
                var response = await client.GetAsync($"api/Orders/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                var order = await response.Content.ReadFromJsonAsync<OrderViewModel>();
                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the order service.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
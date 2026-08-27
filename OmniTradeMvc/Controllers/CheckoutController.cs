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

        // The WebApi requires a Bearer token on Cart/Orders endpoints
        // ([Authorize(Roles = "Customer")]); this was previously never
        // attached, so every call here would have returned 401.
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

        // GET: /Checkout
        // Shows the cart summary before placing the order
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

                var cart = await response.Content.ReadFromJsonAsync<CartViewModel>();

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

        // POST: /Checkout/PlaceOrder
        // Submits the order using the shipping/payment details entered.
        //
        // NOTE: the WebApi's Order model currently has no ShippingAddress
        // or PaymentMethod columns, so those two values are captured here
        // for UX purposes but are not yet persisted server-side. That
        // needs a schema change (Order table + migration) as a follow-up;
        // it's out of scope for this connectivity fix.
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
                // Re-fetch cart so the view isn't empty on validation failure
                var cartResponse = await client.GetAsync($"api/Cart/{customerId.Value}");

                if (cartResponse.IsSuccessStatusCode)
                {
                    model.Cart = await cartResponse.Content.ReadFromJsonAsync<CartViewModel>()
                                 ?? new CartViewModel();
                }

                return View("Index", model);
            }

            try
            {
                // The real WebApi contract: POST api/Orders/checkout/{customerId}
                // with no body - it converts the customer's current cart into
                // an order server-side.
                var response = await client.PostAsync(
                    $"api/Orders/checkout/{customerId.Value}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>();
                    TempData["SuccessMessage"] = "Order placed successfully.";
                    return RedirectToAction(nameof(Confirmation), new { id = createdOrder?.Id });
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(errorBody)
                        ? "Unable to place your order. Please try again."
                        : errorBody);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the order service.");
            }

            var retryCartResponse = await client.GetAsync($"api/Cart/{customerId.Value}");
            if (retryCartResponse.IsSuccessStatusCode)
            {
                model.Cart = await retryCartResponse.Content.ReadFromJsonAsync<CartViewModel>()
                             ?? new CartViewModel();
            }

            return View("Index", model);
        }

        // GET: /Checkout/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var client = GetAuthorizedClient();

            try
            {
                // GET api/Orders/{id} - restricted to the order's own customer
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
using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // TODO: Replace with real logged-in user id once Auth/session is wired up (Harsh's part)
        private const int CurrentCustomerId = 1;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Checkout
        // Shows the cart summary before placing the order
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            try
            {
                // Assumes Shreyas's CartController exposes: GET api/Cart/{customerId}
                var response = await client.GetAsync($"api/Cart/{CurrentCustomerId}");

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
        // Submits the order using the shipping/payment details entered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-fetch cart so the view isn't empty on validation failure
                var client1 = _httpClientFactory.CreateClient("OmniTradeApi");
                var cartResponse = await client1.GetAsync($"api/Cart/{CurrentCustomerId}");

                if (cartResponse.IsSuccessStatusCode)
                {
                    model.Cart = await cartResponse.Content.ReadFromJsonAsync<CartViewModel>()
                                 ?? new CartViewModel();
                }

                return View("Index", model);
            }

            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            var orderRequest = new PlaceOrderRequest
            {
                CustomerId = CurrentCustomerId,
                ShippingAddress = model.ShippingAddress,
                PaymentMethod = model.PaymentMethod
            };

            try
            {
                // Assumes WebApi exposes: POST api/Orders (creates order from customer's current cart)
                var response = await client.PostAsJsonAsync("api/Orders", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>();
                    TempData["SuccessMessage"] = "Order placed successfully.";
                    return RedirectToAction(nameof(Confirmation), new { id = createdOrder?.Id });
                }

                ModelState.AddModelError(string.Empty, "Unable to place your order. Please try again.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the order service.");
            }

            var retryCartResponse = await client.GetAsync($"api/Cart/{CurrentCustomerId}");
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
            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            try
            {
                // Assumes WebApi exposes: GET api/Orders/{id}
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
using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // TODO: Replace with real logged-in user id once Auth/session is wired up (Harsh's part)
        private const int CurrentCustomerId = 1;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Orders
        // Shows the list of orders placed by the current customer ("My Orders")
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            try
            {
                // Assumes WebApi exposes: GET api/Orders/customer/{customerId}
                var response = await client.GetAsync($"api/Orders/customer/{CurrentCustomerId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to load your orders.";
                    return View(new List<OrderViewModel>());
                }

                var orders = await response.Content.ReadFromJsonAsync<List<OrderViewModel>>()
                             ?? new List<OrderViewModel>();

                return View(orders);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the order service.";
                return View(new List<OrderViewModel>());
            }
        }

        // GET: /Orders/Details/5
        // Shows full details for a single order
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            try
            {
                // Assumes WebApi exposes: GET api/Orders/{id}
                var response = await client.GetAsync($"api/Orders/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                var order = await response.Content.ReadFromJsonAsync<OrderViewModel>();

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the order service.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
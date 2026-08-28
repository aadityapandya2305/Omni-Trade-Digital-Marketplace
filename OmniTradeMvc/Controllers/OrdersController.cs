using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;
using OmniTradeMvc.Models;
using System.Net.Http.Json;
using OmniTradeMvc.Services;
using System.Net.Http.Headers;

namespace OmniTradeMvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOrderService _orderService;

        public OrdersController(IHttpClientFactory httpClientFactory,IOrderService orderService)
        {
            _httpClientFactory = httpClientFactory;
            _orderService = orderService;
        }

        [HttpGet]
        [SessionAuthorize("Customer")]
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var token = HttpContext.Session.GetString("Token");

            if (customerId == null || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync($"api/Orders/customer/{customerId.Value}");

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
        [SessionAuthorize("Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("OmniTradeApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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

        // GET: /Orders/Incoming
        // Shows orders containing products belonging to the logged-in vendor
        [HttpGet]
        [SessionAuthorize("Vendor")]
        public async Task<IActionResult> Incoming()
        {
            try
            {
                var vendorId = await _orderService.GetCurrentVendorIdAsync();

                if (vendorId == null)
                {
                    TempData["ErrorMessage"] =
                        "Unable to identify the vendor.";

                    return View(new List<VendorOrderItemViewModel>());
                }

                var orders =
                    await _orderService.GetIncomingOrdersAsync(vendorId.Value);

                return View(orders);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return View(new List<VendorOrderItemViewModel>());
            }
        }

        // GET: /Orders/VendorDetails/5
        // Shows details of an order for the logged-in vendor
        [HttpGet]
        [SessionAuthorize("Vendor")]
        public async Task<IActionResult> VendorDetails(int id)
        {
            try
            {
                var vendorId = await _orderService.GetCurrentVendorIdAsync();

                if (vendorId == null)
                {
                    TempData["ErrorMessage"] =
                        "Unable to identify the vendor.";

                    return RedirectToAction(nameof(Incoming));
                }

                var order =
                    await _orderService.GetOrderDetailsAsync(
                        vendorId.Value,
                        id);

                if (order == null)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(nameof(Incoming));
                }

                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return RedirectToAction(nameof(Incoming));
            }
        }

        // POST: /Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("Vendor")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var success =
                    await _orderService.UpdateOrderStatusAsync(
                        orderId,
                        status);

                if (success)
                {
                    TempData["SuccessMessage"] =
                        "Order status updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Unable to update the order status.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";
            }

            return RedirectToAction(
                nameof(VendorDetails),
                new { id = orderId });
        }
    }
}
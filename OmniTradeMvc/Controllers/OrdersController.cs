using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OmniTradeMvc.Services;

namespace OmniTradeMvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOrderService _orderService;

        public OrdersController(
            IHttpClientFactory httpClientFactory,
            IOrderService orderService)
        {
            _httpClientFactory = httpClientFactory;
            _orderService = orderService;
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

        // GET: /Orders
        // Shows the list of orders placed by the current customer
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
                        $"api/Orders/customer/{customerId.Value}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load your orders.";

                    return View(
                        new List<OrderViewModel>());
                }

                var apiOrders =
                    await response.Content
                        .ReadFromJsonAsync<List<OrderApiResponse>>()
                    ?? new List<OrderApiResponse>();

                var orders =
                    apiOrders
                        .Select(MapOrder)
                        .ToList();

                return View(orders);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return View(
                    new List<OrderViewModel>());
            }
        }

        // GET: /Orders/Details/5
        // Shows full details for a single customer order
        [HttpGet]
        public async Task<IActionResult> Details(int id)
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

                    return RedirectToAction(nameof(Index));
                }

                var apiOrder =
                    await response.Content
                        .ReadFromJsonAsync<OrderApiResponse>();

                if (apiOrder == null)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(nameof(Index));
                }

                var order = MapOrder(apiOrder);

                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Orders/Cancel
        // Cancels a pending order belonging to the logged-in customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
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
                    await client.PostAsync(
                        $"api/Orders/customer/{customerId.Value}/{id}/cancel",
                        null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Order cancelled successfully.";
                }
                else
                {
                    var errorMessage =
                        await response.Content.ReadAsStringAsync();

                    TempData["ErrorMessage"] =
                        string.IsNullOrWhiteSpace(errorMessage)
                            ? "Unable to cancel the order."
                            : errorMessage;
                }

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        // GET: /Orders/Incoming
        // Shows orders containing products belonging to the logged-in vendor
        [HttpGet]
        public async Task<IActionResult> Incoming()
        {
            try
            {
                var vendorId =
                    await _orderService
                        .GetCurrentVendorIdAsync();

                if (vendorId == null)
                {
                    TempData["ErrorMessage"] =
                        "Unable to identify the vendor.";

                    return View(
                        new List<VendorOrderItemViewModel>());
                }

                var orders =
                    await _orderService
                        .GetIncomingOrdersAsync(
                            vendorId.Value);

                return View(orders);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return View(
                    new List<VendorOrderItemViewModel>());
            }
        }

        // GET: /Orders/VendorDetails/5
        // Shows details of an order for the logged-in vendor
        [HttpGet]
        public async Task<IActionResult> VendorDetails(int id)
        {
            try
            {
                var vendorId =
                    await _orderService
                        .GetCurrentVendorIdAsync();

                if (vendorId == null)
                {
                    TempData["ErrorMessage"] =
                        "Unable to identify the vendor.";

                    return RedirectToAction(
                        nameof(Incoming));
                }

                var order =
                    await _orderService
                        .GetOrderDetailsAsync(
                            vendorId.Value,
                            id);

                if (order == null)
                {
                    TempData["ErrorMessage"] =
                        "Order not found.";

                    return RedirectToAction(
                        nameof(Incoming));
                }

                return View(order);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the order service.";

                return RedirectToAction(
                    nameof(Incoming));
            }
        }

        // POST: /Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int orderId,
            string status)
        {
            try
            {
                var success =
                    await _orderService
                        .UpdateOrderStatusAsync(
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

        // Maps the Web API Order response to the MVC OrderViewModel
        private static OrderViewModel MapOrder(
            OrderApiResponse apiOrder)
        {
            return new OrderViewModel
            {
                Id = apiOrder.Id,

                CustomerId = apiOrder.CustomerId,

                OrderDate =
                    apiOrder.OrderDate ?? DateTime.Now,

                Status =
                    apiOrder.Status,

                ShippingAddress =
                    apiOrder.ShippingAddress
                    ?? string.Empty,

                PaymentMethod =
                    apiOrder.PaymentMethod
                    ?? string.Empty,

                TotalAmount =
                    apiOrder.TotalAmount,

                Items =
                    apiOrder.OrderItems
                        .Select(item => new OrderItemViewModel
                        {
                            ProductId =
                                item.ProductId,

                            ProductName =
                                item.Product?.Name
                                ?? "Unknown Product",

                            Price =
                                item.UnitPrice,

                            Quantity =
                                item.Quantity
                        })
                        .ToList()
            };
        }

        // Web API order response
        private class OrderApiResponse
        {
            public int Id { get; set; }

            public int CustomerId { get; set; }

            public DateTime? OrderDate { get; set; }

            public decimal TotalAmount { get; set; }

            public string Status { get; set; } =
                string.Empty;

            public string? ShippingAddress { get; set; }

            public string? PaymentMethod { get; set; }

            public List<OrderApiItem> OrderItems { get; set; } =
                new();
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
using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Net.Http.Json;

namespace OmniTradeMvc.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                var products = await client.GetFromJsonAsync<List<ProductViewModel>>(
                    "api/Products");

                return View(products ?? new List<ProductViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.SearchError = "Unable to connect to the product service.";

                return View(new List<ProductViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                var products = await client.GetFromJsonAsync<List<ProductViewModel>>(
                    $"api/Products/search?name={Uri.EscapeDataString(name)}");

                ViewBag.SearchTerm = name;

                return View("Index", products ?? new List<ProductViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.SearchTerm = name;
                ViewBag.SearchError = "Unable to search products.";

                return View("Index", new List<ProductViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OmniTradeApi");

                var product = await client.GetFromJsonAsync<ProductViewModel>(
                    $"api/Products/{id}");

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }
    }
}
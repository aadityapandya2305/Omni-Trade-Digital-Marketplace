using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;
using OmniTradeMvc.Models;
using OmniTradeMvc.Services;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Vendor")]
    public class VendorsController : Controller
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // GET: /Vendors/Dashboard
        // Overview of the logged-in vendor's store performance
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboard = await _vendorService.GetDashboardAsync();

                if (dashboard == null)
                {
                    TempData["ErrorMessage"] =
                        "No vendor profile found for your account. " +
                        "Please register a store to get started.";

                    return RedirectToAction(nameof(Register));
                }

                return View(dashboard);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the vendor service.";

                return View(new VendorDashboardViewModel());
            }
        }

        // GET: /Vendors/Register
        // Shown to vendors who don't yet have a store profile
        [HttpGet]
        public IActionResult Register()
        {
            return View(new VendorProfileViewModel());
        }

        // POST: /Vendors/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VendorProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _vendorService.RegisterVendorAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to register your store. " +
                        "You may already have a vendor profile.");

                    return View(model);
                }

                TempData["SuccessMessage"] =
                    "Store registered successfully. " +
                    "An admin will review your store for approval.";

                return RedirectToAction(nameof(Dashboard));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the vendor service.");

                return View(model);
            }
        }

        // GET: /Vendors/Profile
        // Edit the logged-in vendor's store profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var profile = await _vendorService.GetProfileAsync();

                if (profile == null)
                {
                    TempData["ErrorMessage"] =
                        "No vendor profile found for your account.";

                    return RedirectToAction(nameof(Register));
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the vendor service.";

                return RedirectToAction(nameof(Dashboard));
            }
        }

        // POST: /Vendors/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(VendorProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _vendorService.UpdateProfileAsync(model);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "Unable to update your store profile.";

                    return RedirectToAction(nameof(Profile));
                }

                TempData["SuccessMessage"] =
                    "Store profile updated successfully.";

                return RedirectToAction(nameof(Dashboard));
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the vendor service.";

                return RedirectToAction(nameof(Profile));
            }
        }

        // GET: /Vendors/Products
        // Lists every product owned by the logged-in vendor
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            try
            {
                var products = await _vendorService.GetMyProductsAsync();

                return View(products);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the product service.";

                return View(Enumerable.Empty<ProductViewModel>());
            }
        }

        // GET: /Vendors/CreateProduct
        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View(new ProductViewModel());
        }

        // POST: /Vendors/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _vendorService.CreateProductAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to create the product. Make sure your " +
                        "store has been approved by an admin.");

                    return View(model);
                }

                TempData["SuccessMessage"] = "Product created successfully.";

                return RedirectToAction(nameof(Products));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the product service.");

                return View(model);
            }
        }

        // GET: /Vendors/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                var product = await _vendorService.GetMyProductAsync(id);

                if (product == null)
                {
                    TempData["ErrorMessage"] =
                        "Product not found or you do not own this product.";

                    return RedirectToAction(nameof(Products));
                }

                return View(product);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the product service.";

                return RedirectToAction(nameof(Products));
            }
        }

        // POST: /Vendors/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(
            int id,
            ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success =
                    await _vendorService.UpdateProductAsync(id, model);

                if (!success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to update the product.");

                    return View(model);
                }

                TempData["SuccessMessage"] = "Product updated successfully.";

                return RedirectToAction(nameof(Products));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the product service.");

                return View(model);
            }
        }

        // POST: /Vendors/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var success = await _vendorService.DeleteProductAsync(id);

                TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                    success
                        ? "Product deleted successfully."
                        : "Unable to delete the product.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the product service.";
            }

            return RedirectToAction(nameof(Products));
        }
    }
}

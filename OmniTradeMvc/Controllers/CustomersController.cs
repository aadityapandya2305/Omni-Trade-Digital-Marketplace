using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Customer")]
    public class CustomersController : Controller
    {
        // GET: /Customers/Dashboard
        // Landing page for the logged-in Customer - independent of
        // Admin/Vendor dashboards, same pattern as AdminController and
        // VendorsController.
        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewData["Username"] = HttpContext.Session.GetString("Username");

            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Diagnostics;

namespace OmniTradeMvc.Controllers
{
    public class HomeController : Controller
    {
        // Public marketing landing page. Same content for everyone,
        // logged in or not - role-based dashboards live at their own
        // dedicated URLs (Admin/Dashboard, Vendors/Dashboard,
        // Customers/Dashboard) and are reached via the nav bar or the
        // post-login redirect in AuthController, not from here.
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Shown by SessionAuthorizeAttribute when a logged-in user's role
        // isn't allowed to access the page they requested.
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
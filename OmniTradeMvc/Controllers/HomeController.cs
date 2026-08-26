using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Models;
using System.Diagnostics;

namespace OmniTradeMvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            switch (role)
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");

                case "Vendor":
                    return RedirectToAction("Dashboard", "Vendors");

                case "Customer":
                    ViewData["Username"] = HttpContext.Session.GetString("Username");
                    return View("CustomerDashboard");

                default:
                    return View();
            }
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

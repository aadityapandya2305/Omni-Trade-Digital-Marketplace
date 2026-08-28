using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Customer")]
    public class CustomersController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewData["Username"] = HttpContext.Session.GetString("Username");

            return View();
        }
    }
}

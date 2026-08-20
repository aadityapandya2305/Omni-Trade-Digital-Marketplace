using Microsoft.AspNetCore.Mvc;

namespace OmniTradeMvc.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
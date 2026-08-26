using Microsoft.AspNetCore.Mvc;
using OmniTradeMvc.Filters;
using OmniTradeMvc.Services;

namespace OmniTradeMvc.Controllers
{
    [SessionAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var analytics =
                await _adminService.GetPlatformAnalyticsAsync();

            if (analytics == null)
            {
                return View();
            }

            return View(analytics);
        }

        public async Task<IActionResult> Users()
        {
            var users =
                await _adminService.GetAllUsersAsync();

            return View(users);
        }

        public async Task<IActionResult> Vendors()
        {
            var vendors =
                await _adminService.GetAllVendorsAsync();

            return View(vendors);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveVendor(
            int id,
            bool isApproved)
        {
            var success =
                await _adminService.UpdateVendorApprovalAsync(
                    id,
                    isApproved);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to update vendor approval status.";

                return RedirectToAction(nameof(Vendors));
            }

            TempData["SuccessMessage"] =
                isApproved
                    ? "Vendor approved successfully."
                    : "Vendor suspended successfully.";

            return RedirectToAction(nameof(Vendors));
        }
    }
}
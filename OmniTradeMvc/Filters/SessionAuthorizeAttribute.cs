using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OmniTradeMvc.Filters
{
    /// <summary>
    /// Guards an MVC controller/action using the same session values that
    /// AuthController.Login sets ("Username", "Role", "Token", "UserId").
    ///
    /// Usage:
    ///   [SessionAuthorize]              // any logged-in user, any role
    ///   [SessionAuthorize("Admin")]     // only the Admin role
    ///   [SessionAuthorize("Vendor")]    // only the Vendor role
    ///
    /// Not logged in            -> redirect to Auth/Login (with returnUrl)
    /// Logged in, wrong role    -> redirect to Home/AccessDenied
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = true)]
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public SessionAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            var username = session.GetString("Username");
            var role = session.GetString("Role");

            var tempData = context.HttpContext.RequestServices
                .GetRequiredService<ITempDataDictionaryFactory>()
                .GetTempData(context.HttpContext);

            // Not logged in at all.
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                tempData["ErrorMessage"] =
                    "Please log in to access that page.";

                context.Result = new RedirectToActionResult(
                    "Login",
                    "Auth",
                    new { returnUrl = context.HttpContext.Request.Path.Value });

                return;
            }

            // Logged in, but not an allowed role for this page.
            if (_allowedRoles.Length > 0 &&
                !_allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                tempData["ErrorMessage"] =
                    "You don't have permission to access that page.";

                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Home",
                    null);

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

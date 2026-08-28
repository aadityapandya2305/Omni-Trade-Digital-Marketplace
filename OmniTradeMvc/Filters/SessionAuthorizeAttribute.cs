using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OmniTradeMvc.Filters
{
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

            var tempData = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context.HttpContext);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                tempData["ErrorMessage"] = "Please log in to access that page.";

                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = context.HttpContext.Request.Path.Value });

                return;
            }

            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                tempData["ErrorMessage"] = "You don't have permission to access that page.";

                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

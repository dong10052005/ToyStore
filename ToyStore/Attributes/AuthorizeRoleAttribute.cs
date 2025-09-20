using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ToyStore.Helpers;

namespace ToyStore.Attributes
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!AuthHelper.IsAuthenticated(context.HttpContext))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (_allowedRoles.Length > 0 && !AuthHelper.HasAnyRole(context.HttpContext, _allowedRoles))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}





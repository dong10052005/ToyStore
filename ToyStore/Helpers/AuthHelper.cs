using ToyStore.Models;

namespace ToyStore.Helpers
{
    public static class AuthHelper
    {
        public static UserSession? GetCurrentUser(HttpContext context)
        {
            return context.Items["UserSession"] as UserSession;
        }

        public static bool IsAuthenticated(HttpContext context)
        {
            return context.Session.GetString("IsAuthenticated") == "True";
        }

        public static bool IsAdmin(HttpContext context)
        {
            return IsAuthenticated(context) && context.Session.GetString("UserType") == "Admin";
        }

        public static bool IsStaff(HttpContext context)
        {
            return IsAuthenticated(context) && (context.Session.GetString("UserType") == "Staff" || context.Session.GetString("UserType") == "Admin");
        }

        public static bool IsCustomer(HttpContext context)
        {
            return IsAuthenticated(context) && context.Session.GetString("UserType") == "Customer";
        }

        public static bool HasRole(HttpContext context, string role)
        {
            return IsAuthenticated(context) && context.Session.GetString("Role") == role;
        }

        public static bool HasAnyRole(HttpContext context, params string[] roles)
        {
            if (!IsAuthenticated(context)) return false;
            
            var userRole = context.Session.GetString("Role");
            return roles.Contains(userRole);
        }
    }
}





using ToyStore.Models;

namespace ToyStore.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy thông tin user từ session
            var userId = context.Session.GetString("UserId");
            var isAuthenticated = context.Session.GetString("IsAuthenticated");

            if (!string.IsNullOrEmpty(userId) && isAuthenticated == "True")
            {
                var userSession = new UserSession
                {
                    UserId = int.Parse(userId),
                    Username = context.Session.GetString("Username") ?? "",
                    Email = context.Session.GetString("Email") ?? "",
                    FullName = context.Session.GetString("FullName") ?? "",
                    UserType = context.Session.GetString("UserType") ?? "",
                    Role = context.Session.GetString("Role") ?? "",
                    IsAuthenticated = true
                };

                // Thêm user session vào HttpContext để sử dụng trong controllers
                context.Items["UserSession"] = userSession;
            }

            await _next(context);
        }
    }

    public static class SessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }
}





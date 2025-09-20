using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Services;

namespace ToyStore.Scripts
{
    public static class CreateDefaultAdmin
    {
        public static async Task CreateAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ToyStoreContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            // Kiểm tra xem đã có admin chưa
            var existingAdmin = await context.Admins.FirstOrDefaultAsync(a => a.Role == "Admin");
            if (existingAdmin != null)
            {
                Console.WriteLine("Admin account already exists.");
                return;
            }

            // Tạo admin mặc định
            var admin = new Admin
            {
                Username = "admin",
                PasswordHash = authService.HashPassword("admin123"),
                FullName = "Quản trị viên hệ thống",
                Role = "Admin"
            };

            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("Default admin account created:");
            Console.WriteLine("Username: admin");
            Console.WriteLine("Password: admin123");
        }
    }
}





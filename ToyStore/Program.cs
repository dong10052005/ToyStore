using ToyStore.Models;
using Microsoft.EntityFrameworkCore;
using ToyStore.Services;
using ToyStore.Extensions;
using ToyStore.Middleware;
using ToyStore.Scripts;

namespace ToyStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Kết nối SQL Server
            builder.Services.AddDbContext<ToyStoreContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ToyStoreDB")));

            // Thêm session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Thêm custom services
            builder.Services.AddCustomServices();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Thêm session middleware
            app.UseSession();
            app.UseSessionMiddleware();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Tạo admin mặc định nếu chưa có
            try
            {
                await CreateDefaultAdmin.CreateAdminAsync(app.Services);
                
                // Chạy test hệ thống (chỉ trong development)
                if (app.Environment.IsDevelopment())
                {
                    await TestAuthSystem.TestAsync(app.Services);
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error creating default admin or running tests");
            }

            app.Run();
        }
    }
}

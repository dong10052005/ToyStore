using Microsoft.AspNetCore.Mvc;
using ToyStore.Models;
using ToyStore.Services;
using System.Diagnostics;

namespace ToyStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Thử đăng nhập với tất cả loại tài khoản
                UserSession? userSession = null;
                
                // Thử đăng nhập Customer trước
                userSession = await _authService.LoginAsync(model.EmailOrUsername, model.Password, "Customer");
                
                // Nếu không thành công, thử Admin/Staff
                if (userSession == null)
                {
                    userSession = await _authService.LoginAsync(model.EmailOrUsername, model.Password, "Admin");
                }
                
                if (userSession == null)
                {
                    ModelState.AddModelError("", "Email/Username hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Lưu thông tin user vào session
                HttpContext.Session.SetString("UserId", userSession.UserId.ToString());
                HttpContext.Session.SetString("Username", userSession.Username);
                HttpContext.Session.SetString("Email", userSession.Email);
                HttpContext.Session.SetString("FullName", userSession.FullName);
                HttpContext.Session.SetString("UserType", userSession.UserType);
                HttpContext.Session.SetString("Role", userSession.Role);
                HttpContext.Session.SetString("IsAuthenticated", userSession.IsAuthenticated.ToString());

                _logger.LogInformation($"User {userSession.Username} logged in successfully");

                // Redirect dựa trên loại user
                return userSession.UserType switch
                {
                    "Admin" => RedirectToAction("Index", "Home"),
                    "Staff" => RedirectToAction("Index", "Home"),
                    "Customer" => RedirectToAction("Index", "Home"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng nhập");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email đã tồn tại chưa
                if (await _authService.IsEmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    return View(model);
                }

                var result = await _authService.RegisterAsync(model);
                
                if (!result)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

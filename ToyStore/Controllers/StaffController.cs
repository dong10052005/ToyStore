using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Services;
using ToyStore.Attributes;
using ToyStore.Helpers;

namespace ToyStore.Controllers
{
    [AuthorizeRole("Admin")]
    public class StaffController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ToyStoreContext _context;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IAuthService authService, ToyStoreContext context, ILogger<StaffController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var staff = await _context.Admins
                .Where(a => a.Role != "Admin")
                .OrderBy(a => a.FullName)
                .ToListAsync();
            
            return View(staff);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra username đã tồn tại chưa
                if (await _authService.IsUsernameExistsAsync(model.Username))
                {
                    ModelState.AddModelError("Username", "Username này đã được sử dụng");
                    return View(model);
                }

                var result = await _authService.CreateStaffAsync(model);
                
                if (!result)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình tạo tài khoản nhân viên");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Tạo tài khoản nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff account");
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình tạo tài khoản nhân viên");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var staff = await _context.Admins.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.Admins.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            var model = new CreateStaffViewModel
            {
                FullName = staff.FullName ?? "",
                Username = staff.Username,
                Role = staff.Role ?? "Staff"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateStaffViewModel model)
        {
            if (id != model.GetHashCode()) // Simplified check
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var staff = await _context.Admins.FindAsync(id);
                if (staff == null)
                {
                    return NotFound();
                }

                // Kiểm tra username đã tồn tại chưa (trừ chính nó)
                var existingStaff = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == model.Username && a.AdminId != id);
                
                if (existingStaff != null)
                {
                    ModelState.AddModelError("Username", "Username này đã được sử dụng");
                    return View(model);
                }

                staff.FullName = model.FullName;
                staff.Username = model.Username;
                staff.Role = model.Role;

                // Chỉ cập nhật mật khẩu nếu có nhập
                if (!string.IsNullOrEmpty(model.Password))
                {
                    staff.PasswordHash = _authService.HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff account");
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình cập nhật thông tin nhân viên");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.Admins.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var staff = await _context.Admins.FindAsync(id);
                if (staff != null)
                {
                    _context.Admins.Remove(staff);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa tài khoản nhân viên thành công!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff account");
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình xóa tài khoản nhân viên";
            }

            return RedirectToAction("Index");
        }
    }
}





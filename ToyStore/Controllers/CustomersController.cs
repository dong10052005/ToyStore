using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Attributes;
using ToyStore.Helpers;
using ToyStore.Services;

namespace ToyStore.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ToyStoreContext _context;
        private readonly IAuthService _authService;

        public CustomersController(ToyStoreContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: Customers
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,Email,Phone,Address,PasswordHash,CreatedAt")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,Address,PasswordHash,CreatedAt")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        // Customer Profile Management Actions
        [AuthorizeRole("Customer")]
        public async Task<IActionResult> MyProfile()
        {
            var userSession = AuthHelper.GetCurrentUser(HttpContext);
            if (userSession == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var customer = await _context.Customers.FindAsync(userSession.UserId);
            if (customer == null)
            {
                return NotFound();
            }

            var viewModel = new EditCustomerViewModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Customer")]
        public async Task<IActionResult> MyProfile(EditCustomerViewModel model)
        {
            var userSession = AuthHelper.GetCurrentUser(HttpContext);
            if (userSession == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (userSession.UserId != model.CustomerId)
            {
                return Forbid();
            }

            // Kiểm tra email có trùng với customer khác không
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == model.Email && c.CustomerId != model.CustomerId);
            if (existingCustomer != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác");
            }

            if (ModelState.IsValid)
            {
                var customer = await _context.Customers.FindAsync(model.CustomerId);
                if (customer == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin cơ bản
                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;

                // Xử lý đổi mật khẩu nếu có
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu");
                        return View(model);
                    }

                    // Kiểm tra mật khẩu hiện tại
                    if (!_authService.VerifyPassword(model.CurrentPassword, customer.PasswordHash))
                    {
                        ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                        return View(model);
                    }

                    // Cập nhật mật khẩu mới
                    customer.PasswordHash = _authService.HashPassword(model.NewPassword);
                }

                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(MyProfile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

    }
}

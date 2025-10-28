using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Attributes;

namespace ToyStore.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class OrdersController : Controller
    {
        private readonly ToyStoreContext _context;

        public OrdersController(ToyStoreContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchCustomer, string searchDate)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            // Filter by customer name if provided
            if (!string.IsNullOrEmpty(searchCustomer))
            {
                ordersQuery = ordersQuery.Where(o => o.Customer.FullName.Contains(searchCustomer));
            }

            // Filter by date if provided
            if (!string.IsNullOrEmpty(searchDate))
            {
                if (DateTime.TryParse(searchDate, out DateTime parsedDate))
                {
                    ordersQuery = ordersQuery.Where(o => o.OrderDate.HasValue && 
                        o.OrderDate.Value.Date == parsedDate.Date);
                }
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.SearchCustomer = searchCustomer;
            ViewBag.SearchDate = searchDate;
            
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,TotalAmount,Status,PaymentMethod,DeliveryMethod")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,TotalAmount,Status,PaymentMethod,DeliveryMethod")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // First, check if order exists
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                // Check if order has order details
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == id)
                    .ToListAsync();

                if (orderDetails.Any())
                {
                    // Delete order details first
                    _context.OrderDetails.RemoveRange(orderDetails);
                    
                    // Then delete the order
                    _context.Orders.Remove(order);
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã xóa đơn hàng #{id} và {orderDetails.Count} chi tiết sản phẩm thành công!";
                }
                else
                {
                    // Safe to delete - remove the order
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database constraint violations
                TempData["ErrorMessage"] = "Không thể xóa đơn hàng vì đơn hàng đang được sử dụng trong hệ thống. Đơn hàng sẽ được giữ lại để lưu trữ.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Orders/Confirm/5
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                if (order.Status == "Confirmed")
                {
                    TempData["WarningMessage"] = "Đơn hàng đã được xác nhận trước đó";
                    return RedirectToAction("Index");
                }

                // Update order status to confirmed
                order.Status = "Confirmed";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xác nhận đơn hàng #{order.OrderId} thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Orders/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                    return RedirectToAction("Index");
                }

                if (order.Status == "Cancelled")
                {
                    TempData["WarningMessage"] = "Đơn hàng đã được hủy trước đó";
                    return RedirectToAction("Index");
                }

                // Update order status to cancelled
                order.Status = "Cancelled";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{order.OrderId} thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}

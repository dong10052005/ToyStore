using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Attributes;

namespace ToyStore.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class ProductsController : Controller
    {
        private readonly ToyStoreContext _context;

        public ProductsController(ToyStoreContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(product.ProductName))
                {
                    TempData["ErrorMessage"] = "Tên sản phẩm không được để trống";
                    var categories = await _context.Categories.ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                if (product.CategoryId <= 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn danh mục";
                    var categories = await _context.Categories.ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                // Set default values
                if (product.Price <= 0) product.Price = 0;
                if (product.Stock <= 0) product.Stock = 0;
                if (product.Status == null) product.Status = true;

                // Add product
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            var categoriesList = await _context.Categories.ToListAsync();
            ViewBag.Categories = categoriesList;
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            try
            {
                // Check if product exists
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }

                // Validate required fields
                if (string.IsNullOrEmpty(product.ProductName))
                {
                    TempData["ErrorMessage"] = "Tên sản phẩm không được để trống";
                    var categories = await _context.Categories.ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                if (product.CategoryId <= 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn danh mục";
                    var categories = await _context.Categories.ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                // Update existing product properties
                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Status = product.Status;

                // Update product
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            var categoriesList = await _context.Categories.ToListAsync();
            ViewBag.Categories = categoriesList;
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // First, check if product exists
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }

                // Check if product is used in orders
                bool hasOrderDetails = await _context.OrderDetails
                    .AnyAsync(od => od.ProductId == id);

                // Check if product is used in cart items
                bool hasCartItems = await _context.CartItems
                    .AnyAsync(ci => ci.ProductId == id);

                if (hasOrderDetails || hasCartItems)
                {
                    // Instead of deleting, set status to false (not selling)
                    product.Status = false;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                    
                    string reason = "";
                    if (hasOrderDetails && hasCartItems)
                        reason = "Sản phẩm đã được sử dụng trong đơn hàng và giỏ hàng";
                    else if (hasOrderDetails)
                        reason = "Sản phẩm đã được sử dụng trong đơn hàng";
                    else
                        reason = "Sản phẩm đang có trong giỏ hàng";
                    
                    TempData["SuccessMessage"] = $"Không thể xóa sản phẩm vì {reason}. Đã chuyển trạng thái thành 'Không bán'.";
                }
                else
                {
                    // Safe to delete - remove the product
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database constraint violations
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì sản phẩm đang được sử dụng trong hệ thống. Đã chuyển trạng thái thành 'Không bán'.";
                
                // Try to set status to false instead
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product != null)
                    {
                        product.Status = false;
                        _context.Products.Update(product);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đã chuyển trạng thái sản phẩm thành 'Không bán'.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
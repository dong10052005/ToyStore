using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Attributes;

namespace ToyStore.Controllers
{
    [AuthorizeRole("Customer")]
    public class OrderController : Controller
    {
        private readonly ToyStoreContext _context;
        private const string CART_SESSION_KEY = "ShoppingCart";

        public OrderController(ToyStoreContext context)
        {
            _context = context;
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            try
            {
                var cart = GetCart();
                
                if (!cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống";
                    return RedirectToAction("Index", "Cart");
                }

                // Check if customer is logged in
                var customerId = GetCurrentCustomerId();
                if (customerId == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để đặt hàng";
                    return RedirectToAction("Login", "Auth");
                }

                // Create order with basic properties only (using existing columns)
                var newOrder = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    TotalAmount = cart.Total,
                    PaymentMethod = order.PaymentMethod ?? "COD"
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // Create order details
                foreach (var item in cart.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    
                    _context.OrderDetails.Add(orderDetail);

                    // Update product stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        if (product.Stock < 0) product.Stock = 0;
                    }
                }

                await _context.SaveChangesAsync();

                // Clear cart
                cart.Clear();
                SaveCart(cart);

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{newOrder.OrderId}";
                return RedirectToAction("Details", new { id = newOrder.OrderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message + " - Chi tiết: " + (ex.InnerException?.Message ?? "");
                return RedirectToAction("Checkout", "Cart");
            }
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || order.CustomerId != GetCurrentCustomerId())
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var customerId = GetCurrentCustomerId();
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: Order/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == GetCurrentCustomerId());

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                    return RedirectToAction("MyOrders");
                }

                if (order.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng đang chờ xử lý";
                    return RedirectToAction("MyOrders");
                }

                // Restore product stock
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                    }
                }

                order.Status = "Cancelled";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công";
                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("MyOrders");
            }
        }

        private ShoppingCart GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CART_SESSION_KEY);
            return ShoppingCart.FromJson(cartJson ?? "");
        }

        private void SaveCart(ShoppingCart cart)
        {
            var cartJson = cart.ToJson();
            HttpContext.Session.SetString(CART_SESSION_KEY, cartJson);
        }

        private int GetCurrentCustomerId()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userId, out int id))
            {
                return id;
            }
            return 0;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Attributes;

namespace ToyStore.Controllers
{
    [AuthorizeRole("Customer")]
    public class CartController : Controller
    {
        private readonly ToyStoreContext _context;
        private const string CART_SESSION_KEY = "ShoppingCart";

        public CartController(ToyStoreContext context)
        {
            _context = context;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index", "Home");
                }

                if (product.Status != true)
                {
                    TempData["ErrorMessage"] = "Sản phẩm hiện không được bán";
                    return RedirectToAction("Index", "Home");
                }

                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0";
                    return RedirectToAction("Index", "Home");
                }

                var cart = GetCart();
                cart.AddItem(product, quantity);
                SaveCart(cart);

                TempData["SuccessMessage"] = $"Đã thêm {product.ProductName} vào giỏ hàng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Cart/Update
        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            try
            {
                var cart = GetCart();
                cart.UpdateQuantity(productId, quantity);
                SaveCart(cart);

                TempData["SuccessMessage"] = "Đã cập nhật giỏ hàng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                
                if (item != null)
                {
                    cart.RemoveItem(productId);
                    SaveCart(cart);
                    TempData["SuccessMessage"] = $"Đã xóa {item.ProductName} khỏi giỏ hàng";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            try
            {
                var cart = GetCart();
                cart.Clear();
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = GetCart();
            
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống, không thể thanh toán";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Pending",
                CustomerId = GetCurrentCustomerId()
            };

            return View(order);
        }

        // GET: Cart/GetCount
        public IActionResult GetCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.ItemCount });
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

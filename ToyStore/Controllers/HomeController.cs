using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToyStore.Models;
using ToyStore.Helpers;

namespace ToyStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToyStoreContext _context;

        public HomeController(ILogger<HomeController> logger, ToyStoreContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = AuthHelper.GetCurrentUser(HttpContext);
            ViewBag.User = user;
            
            // Lấy danh sách sản phẩm được nhóm theo danh mục
            var categoriesWithProducts = await _context.Categories
                .Include(c => c.Products.Where(p => p.Status == true))
                .Where(c => c.Products.Any(p => p.Status == true))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            return View(categoriesWithProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Shop()
        {
            // Lấy danh sách sản phẩm được nhóm theo danh mục
            var categoriesWithProducts = await _context.Categories
                .Include(c => c.Products.Where(p => p.Status == true))
                .Where(c => c.Products.Any(p => p.Status == true))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            return View(categoriesWithProducts);
        }

        // GET: Home/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var user = AuthHelper.GetCurrentUser(HttpContext);
            ViewBag.User = user;
            
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

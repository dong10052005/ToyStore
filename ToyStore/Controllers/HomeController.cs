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

        public async Task<IActionResult> Index(string searchName)
        {
            var user = AuthHelper.GetCurrentUser(HttpContext);
            ViewBag.User = user;
            
            // Check if user just logged in
            var showWelcomeToast = HttpContext.Session.GetString("ShowWelcomeToast");
            if (!string.IsNullOrEmpty(showWelcomeToast))
            {
                HttpContext.Session.Remove("ShowWelcomeToast");
                TempData["ShowWelcomeToast"] = "true";
            }
            
            var categoriesQuery = _context.Categories
                .Include(c => c.Products.Where(p => p.Status == true))
                .Where(c => c.Products.Any(p => p.Status == true))
                .AsQueryable();
            
            // Filter products by name if search is provided
            if (!string.IsNullOrEmpty(searchName))
            {
                categoriesQuery = categoriesQuery
                    .Where(c => c.Products.Any(p => p.Status == true && p.ProductName.Contains(searchName)));
            }
            
            var categoriesWithProducts = await categoriesQuery
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            // Filter products within each category
            if (!string.IsNullOrEmpty(searchName))
            {
                foreach (var category in categoriesWithProducts)
                {
                    category.Products = category.Products
                        .Where(p => p.ProductName.Contains(searchName))
                        .ToList();
                }
            }
            
            ViewBag.SearchName = searchName;
            return View(categoriesWithProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Shop(string searchName, int? categoryId)
        {
            var categoriesQuery = _context.Categories
                .Include(c => c.Products.Where(p => p.Status == true))
                .AsQueryable();
            
            // Filter by category if provided
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                categoriesQuery = categoriesQuery.Where(c => c.CategoryId == categoryId.Value);
            }
            
            // Only include categories that have products
            categoriesQuery = categoriesQuery.Where(c => c.Products.Any(p => p.Status == true));
            
            var categoriesWithProducts = await categoriesQuery
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            // Filter products by name if search is provided
            if (!string.IsNullOrEmpty(searchName))
            {
                foreach (var category in categoriesWithProducts)
                {
                    category.Products = category.Products
                        .Where(p => p.ProductName.Contains(searchName))
                        .ToList();
                }
                
                // Remove categories that have no matching products
                categoriesWithProducts = categoriesWithProducts
                    .Where(c => c.Products.Any())
                    .ToList();
            }
            
            // Get all categories for filter dropdown
            var allCategories = await _context.Categories
                .Include(c => c.Products.Where(p => p.Status == true))
                .Where(c => c.Products.Any(p => p.Status == true))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            ViewBag.Categories = allCategories;
            ViewBag.SearchName = searchName;
            ViewBag.CategoryId = categoryId;
            
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

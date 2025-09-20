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

        public IActionResult Index()
        {
            var user = AuthHelper.GetCurrentUser(HttpContext);
            ViewBag.User = user;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Shop()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == true)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();
            
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

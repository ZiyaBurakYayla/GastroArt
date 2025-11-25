using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models.Data;
using GastroArt.Models.Entity;

namespace GastroArt.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"))) return RedirectToAction("Login", "Admin");
            return View();
        }

        [HttpGet]
        public IActionResult GetChartData()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"))) return Unauthorized();

            // Son 7 Günlük Satış
            var last7Days = DateTime.Today.AddDays(-6);
            var salesData = _context.Orders
                .Where(o => o.OrderDate >= last7Days && o.OrderState == OrderState.Completed)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key.ToString("dd.MM"), Total = g.Sum(x => x.TotalAmount) })
                .ToList();

            var finalSales = new List<object>();
            for (int i = 0; i < 7; i++)
            {
                var date = last7Days.AddDays(i);
                var dayData = salesData.FirstOrDefault(x => x.Date == date.ToString("dd.MM"));
                finalSales.Add(new { date = date.ToString("dd.MM"), total = dayData != null ? dayData.Total : 0 });
            }

            // Top 5 Ürün
            var categoryData = _context.OrderItems
                .Include(i => i.Order)
                .Where(i => i.Order.OrderState == OrderState.Completed)
                .GroupBy(i => i.ProductName)
                .Select(g => new { Name = g.Key, Count = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            return Json(new { dailySales = finalSales, topProducts = categoryData });
        }
    }
}
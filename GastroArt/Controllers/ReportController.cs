using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models;
using GastroArt.Models.Data;

namespace GastroArt.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        // ÜRÜN SATIŞ RAPORLARI
        public IActionResult Reports(DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"))) return RedirectToAction("Login","Admin");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Now; 

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var reportData = _context.OrderItems
                .Include(x => x.Order)
                .Where(x => x.Order.OrderDate >= start &&
                            x.Order.OrderDate <= end &&
                            x.Order.OrderState == GastroArt.Models.Entity.OrderState.Completed)
                .GroupBy(x => x.ProductName)
                .Select(g => new ProductReportViewModel
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalIncome = g.Sum(x => x.Price * x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return View(reportData);
        }
    }
}

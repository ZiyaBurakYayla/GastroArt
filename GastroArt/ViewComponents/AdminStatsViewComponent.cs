using Microsoft.AspNetCore.Mvc;
using GastroArt.Models.Data;

public class AdminStatsViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    public AdminStatsViewComponent(AppDbContext context) { _context = context; }

    public IViewComponentResult Invoke()
    {
        var stats = new
        {
            CategoryCount = _context.Categories.Count(),
            ProductCount = _context.Products.Count(),
            ActiveCount = _context.Products.Where(p => p.IsActive).Count()
        };
        return View(stats);
    }
}
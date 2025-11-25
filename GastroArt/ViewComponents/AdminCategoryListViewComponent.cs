using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models.Data;

namespace QrKodMenu.ViewComponents
{
    public class AdminCategoryListViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public AdminCategoryListViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Kategorileri ürünleriyle birlikte çek
            var categories = _context.Categories
                .Include(x => x.Products)
                .OrderBy(x => x.Id)
                .ToList();

            return View(categories);
        }
    }
}
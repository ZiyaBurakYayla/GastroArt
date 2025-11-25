using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models.Data; 
using GastroArt.Models.Entity;

namespace GastroArt.ViewComponents
{
    public class AdminProductListViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public AdminProductListViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(string search, int? categoryId)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCatId = categoryId;

            return View(productsQuery.OrderByDescending(p => p.Id).ToList());
        }
    }
}
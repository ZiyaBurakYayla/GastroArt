using Microsoft.AspNetCore.Mvc;
using GastroArt.Models.Data;
using GastroArt.Models.Entity;

namespace GastroArt.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"))) return RedirectToAction("Login", "Admin");
            return View();
        }

        [HttpPost]
        public IActionResult Create(string Name, string NameEn, string ColorCode)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                _context.Categories.Add(new Category { Name = Name, NameEn = NameEn, ColorCode = ColorCode, IsActive = true });
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int Id, string Name, string NameEn, string ColorCode)
        {
            var cat = _context.Categories.Find(Id);
            if (cat != null)
            {
                cat.Name = Name;
                cat.NameEn = NameEn;
                cat.ColorCode = ColorCode;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var cat = _context.Categories.Find(id);
            if (cat != null) { _context.Categories.Remove(cat); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }
    }
}
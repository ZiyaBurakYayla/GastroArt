using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using GastroArt.Models.Data;
using GastroArt.Models.Entity;
using System.Globalization;
using System.IO; 

namespace   GastroArt.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private async Task<string> UploadFile(IFormFile file)
        {
            string fileName = null;
            if (file != null)
            {
                string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images/products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                fileName = Guid.NewGuid().ToString() + "-" + file.FileName;
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return fileName != null ? "/images/products/" + fileName : null;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int CategoryId, string Name, string Price, string Description, IFormFile ImageFile, int Stock, bool TrackStock, string NameEn, string DescriptionEn)
        {
            decimal finalPrice = 0;
            if (!string.IsNullOrEmpty(Price))
            {
                string cleanPrice = Price.Replace(",", ".");
                decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out finalPrice);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                string imagePath = await UploadFile(ImageFile);
                if (string.IsNullOrEmpty(imagePath)) imagePath = "https://via.placeholder.com/150";

                var newProduct = new Product
                {
                    CategoryId = CategoryId,
                    Name = Name,
                    NameEn = NameEn,
                    Price = finalPrice,
                    Description = Description,
                    DescriptionEn = DescriptionEn,
                    ImageUrl = imagePath,
                    Stock = Stock,
                    TrackStock = TrackStock,
                    IsActive = true
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, int CategoryId, string Name, string Price, string Description, IFormFile ImageFile, bool IsActive, int Stock, bool TrackStock, string NameEn, string DescriptionEn)
        {
            var product = _context.Products.Find(Id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(Price))
                {
                    string cleanPrice = Price.Replace(",", ".");
                    decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal finalPrice);
                    product.Price = finalPrice;
                }

                if (ImageFile != null)
                {
                    product.ImageUrl = await UploadFile(ImageFile);
                }

                product.CategoryId = CategoryId;
                product.Name = Name;
                product.NameEn = NameEn;
                product.Description = Description;
                product.DescriptionEn = DescriptionEn;
                product.IsActive = IsActive;
                product.Stock = Stock;
                product.TrackStock = TrackStock;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Index(string search, int? categoryId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser")))
            {
                return RedirectToAction("Login", "Admin");
            }
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            return View();
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
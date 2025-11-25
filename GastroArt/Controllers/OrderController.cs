using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models.Data;
using GastroArt.Models.Entity;
using static GastroArt.Controllers.MenuController;

namespace GastroArt.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        private bool CheckSession()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"));
        }

        // 1. SİPARİŞLER SAYFASI
        public IActionResult Index()
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);
            ViewBag.ActivePeriod = activePeriod;

            decimal currentRevenue = 0;
            if (activePeriod != null)
            {
                currentRevenue = _context.Orders
                    .Where(x => x.WorkPeriodId == activePeriod.Id && x.OrderState == OrderState.Completed)
                    .Sum(x => x.TotalAmount);
            }
            ViewBag.CurrentRevenue = currentRevenue; 

            var orders = _context.Orders
                .Include(x => x.OrderItems)
                .Where(x => x.OrderState == OrderState.Pending)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }

        // 2. SİPARİŞİ TAMAMLA
        [HttpPost]
        public IActionResult Complete(int id)
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);
            if (activePeriod == null)
            {
                TempData["Error"] = "⚠️ İŞLEM BAŞARISIZ: Sistem (Kasa) KAPALI! Siparişi tamamlamak için önce günü başlatmalısınız.";
                return RedirectToAction("Index");
            }

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.OrderState = OrderState.Completed;
                order.WorkPeriodId = activePeriod.Id; 
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 3. SİPARİŞİ İPTAL ET (GÜNCELLENDİ: Kasa Kontrolü)
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);
            if (activePeriod == null)
            {
                TempData["Error"] = "⚠️ İŞLEM BAŞARISIZ: Sistem (Kasa) KAPALI! İşlem yapmak için günü başlatın.";
                return RedirectToAction("Index");
            }

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.OrderState = OrderState.Canceled;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 4. GEÇMİŞ & İPTAL EDİLEN SİPARİŞLER SAYFASI (ARŞİV)
        public IActionResult OrderHistory()
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var archives = _context.Orders
                .Include(x => x.OrderItems)
                .Where(x => x.OrderState != OrderState.Pending)
                .OrderByDescending(x => x.OrderDate)
                .Take(100)
                .ToList();

            return View(archives);
        }

        // 5. GERİ AL (DÜZELTİLDİ: Kasa Kontrolü Eklendi)
        [HttpPost]
        public IActionResult Undo(int id)
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);
            if (activePeriod == null)
            {
                TempData["Error"] = "⚠️ İŞLEM BAŞARISIZ: Kasa (Sistem) şu an KAPALI! Geçmiş bir siparişi işleme almak için önce günü başlatmalısınız.";
                return RedirectToAction("Index");
            }

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                var hoursPassed = (DateTime.Now - order.OrderDate).TotalHours;
                if (hoursPassed > 5)
                {
                    TempData["Error"] = "5 saati geçen siparişler geri alınamaz!";
                    return RedirectToAction("Index");
                }

                order.OrderState = GastroArt.Models.Entity.OrderState.Pending; 
                order.WorkPeriodId = null; 
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // --- KASA / SİSTEM YÖNETİMİ ---

        [HttpPost]
        public IActionResult ToggleSystem()
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin"); // GÜVENLİK

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);

            if (activePeriod != null)
            {
                // KAPAT
                activePeriod.IsActive = false;
                activePeriod.EndTime = DateTime.Now;

                var revenue = _context.Orders
                    .Where(x => x.WorkPeriodId == activePeriod.Id && x.OrderState == OrderState.Completed)
                    .Sum(x => x.TotalAmount);

                activePeriod.TotalRevenue = revenue;
            }
            else
            {
                // AÇ
                var newPeriod = new WorkPeriod
                {
                    StartTime = DateTime.Now,
                    IsActive = true,
                    TotalRevenue = 0
                };
                _context.WorkPeriods.Add(newPeriod);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult PeriodHistory()
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin"); // GÜVENLİK

            var history = _context.WorkPeriods
                .OrderByDescending(x => x.Id)
                .ToList();
            return View(history);
        }

        // 6. GARSON İÇİN SİPARİŞ EKRANI (POS)
        public IActionResult Create()
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var categories = _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .ToList();

            return View(categories);
        }

        // 7. GARSON SİPARİŞİNİ KAYDET
        [HttpPost]
        public IActionResult PlaceAdminOrder([FromBody] OrderRequestViewModel model)
        {
            if (!CheckSession()) return Json(new { success = false, message = "Oturum süresi doldu." });

            var activePeriod = _context.WorkPeriods.FirstOrDefault(x => x.IsActive);
            if (activePeriod == null)
            {
                return Json(new { success = false, message = "Kasa (Vardiya) kapalı! Sipariş girilemez." });
            }

            if (model != null && model.Items.Count > 0)
            {
                var order = new Order
                {
                    TableNo = model.TableNo, 
                    CustomerNote = "Garson Siparişi: " + (model.CustomerNote ?? ""),
                    TotalAmount = model.Items.Sum(x => x.Price * x.Quantity),
                    OrderDate = DateTime.Now,
                    OrderState = OrderState.Pending,
                    WorkPeriodId = null 
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in model.Items)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }
                _context.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Sepet boş!" });
        }

        // 8. PERİYOT DETAYLARINI GETİR (AJAX İÇİN)
        [HttpGet]
        public IActionResult GetPeriodDetails(int id)
        {
            if (!CheckSession()) return Unauthorized();

            
            var sales = _context.Orders
                .Where(o => o.WorkPeriodId == id && o.OrderState == OrderState.Completed) 
                .SelectMany(o => o.OrderItems) 
                .GroupBy(i => i.ProductName)  
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Total = g.Sum(x => x.Price * x.Quantity) 
                })
                .OrderByDescending(x => x.Total) 
                .ToList();

            return Json(sales);
        }
        // 9. ADİSYON YAZDIRMA EKRANI
        public IActionResult Print(int id)
        {
            if (!CheckSession()) return RedirectToAction("Login", "Admin");

            var order = _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefault(x => x.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
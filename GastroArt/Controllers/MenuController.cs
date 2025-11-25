using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using GastroArt.Hubs;
using GastroArt.Models.Data;
using System.Text.Json; 

namespace GastroArt.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public MenuController(AppDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Index(string tableId)
        {
            if (!string.IsNullOrEmpty(tableId))
            {
                HttpContext.Session.SetString("TableNo", tableId);
            }

            ViewBag.TableNo = HttpContext.Session.GetString("TableNo");

            var data = _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    nameEn = c.NameEn,
                    color = c.ColorCode,
                    products = c.Products.Where(p => p.IsActive).Select(p => new
                    {
                        name = p.Name,
                        nameEn = p.NameEn,
                        description = p.Description,
                        descriptionEn = p.DescriptionEn,
                        price = p.Price,
                        image = p.ImageUrl,
                        stock = p.Stock,
                        trackStock = p.TrackStock
                    }).ToList()
                })
                .ToList();

            string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return View("Index", jsonString);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestViewModel model)
        {
            if (model != null && model.Items.Count > 0)
            {
                var order = new Order
                {
                    TableNo = model.TableNo,
                    CustomerNote = model.CustomerNote,
                    TotalAmount = model.Items.Sum(x => x.Price * x.Quantity),
                    OrderDate = DateTime.Now,
                    IsCompleted = false,
                    OrderState = GastroArt.Models.Entity.OrderState.Pending
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

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
                await _context.SaveChangesAsync();

                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveOrder", new
                    {
                        id = order.Id,
                        tableNo = order.TableNo,
                        total = order.TotalAmount,
                        date = order.OrderDate.ToString("HH:mm"),
                        note = order.CustomerNote,
                        items = model.Items
                    });
                }

                return Ok(new { success = true });
            }
            return BadRequest(new { success = false });
        }

        public class OrderRequestViewModel
        {
            public string TableNo { get; set; }
            public string CustomerNote { get; set; }
            public List<CartItemViewModel> Items { get; set; }
        }

        public class CartItemViewModel
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
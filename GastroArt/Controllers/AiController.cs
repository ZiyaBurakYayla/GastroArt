using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GastroArt.Models.Data;
using GastroArt.Models.Entity;
using System.Text;

namespace GastroArt.Controllers
{
    public class AiController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AiController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> AskAi([FromBody] AiRequest request)
        {
            if (string.IsNullOrEmpty(request.Message)) return BadRequest(new { error = "Mesaj boş olamaz." });
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"))) return Unauthorized();

            try
            {
                var sessionHistory = HttpContext.Session.GetString("AiChatHistory");
                var history = string.IsNullOrEmpty(sessionHistory) ? new List<ChatMessage>() : JsonConvert.DeserializeObject<List<ChatMessage>>(sessionHistory);

                history.Add(new ChatMessage { Role = "user", Text = request.Message });

                var systemPrompt = await GetSystemPrompt();

                var contentsList = history.Select(h => new { role = h.Role, parts = new[] { new { text = h.Text } } }).ToList();

                var jsonPayload = new
                {
                    systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
                    contents = contentsList
                };

                var apiKey = _configuration["Gemini:ApiKey"];
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(jsonPayload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_apiUrl}?key={apiKey}", content);

                    if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode, new { error = "API Hatası" });

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(jsonResponse)!;

                    if (data.candidates == null || data.candidates.Count == 0) return Ok(new { answer = "Cevap yok." });

                    string rawAnswer = data.candidates[0].content.parts[0].text;
                    rawAnswer = rawAnswer.Trim();

                    rawAnswer = rawAnswer.Replace("```json", "").Replace("```", "").Trim();

                    List<AiCommand> commands = new List<AiCommand>();

                    try
                    {
                        if (rawAnswer.StartsWith("{"))
                        {
                            var singleCmd = JsonConvert.DeserializeObject<AiCommand>(rawAnswer);
                            if (singleCmd != null) commands.Add(singleCmd);
                        }
                        else if (rawAnswer.StartsWith("["))
                        {
                            var listCmds = JsonConvert.DeserializeObject<List<AiCommand>>(rawAnswer);
                            if (listCmds != null) commands.AddRange(listCmds);
                        }
                        else if (rawAnswer.Contains("}{"))
                        {
                            var fixedJson = "[" + rawAnswer.Replace("}\n{", "},{").Replace("}{", "},{") + "]";
                            var listCmds = JsonConvert.DeserializeObject<List<AiCommand>>(fixedJson);
                            if (listCmds != null) commands.AddRange(listCmds);
                        }
                    }
                    catch { }

                    if (commands.Count > 0)
                    {
                        StringBuilder results = new StringBuilder();
                        foreach (var cmd in commands)
                        {
                            if (!string.IsNullOrEmpty(cmd.command))
                            {
                                string res = await ExecuteCommand(cmd);
                                results.AppendLine(res);
                            }
                        }
                        return await GetFinalAnswerFromAi(request.Message, results.ToString());
                    }

                    history.Add(new ChatMessage { Role = "model", Text = rawAnswer });
                    HttpContext.Session.SetString("AiChatHistory", JsonConvert.SerializeObject(history));

                    return Ok(new { answer = rawAnswer });
                }
            }
            catch (Exception ex) { return BadRequest(new { error = "Hata: " + ex.Message }); }
        }

        private async Task<string> ExecuteCommand(AiCommand cmd)
        {
            try
            {
                switch (cmd.command)
                {
                    case "UpdateStock":
                        var prod = await _context.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == cmd.product.ToLower());
                        if (prod == null) return $"- Hata: '{cmd.product}' bulunamadı.";
                        prod.Stock = cmd.stock;
                        await _context.SaveChangesAsync();
                        return $"- Başarılı: {cmd.product} stoğu {cmd.stock} oldu.";

                    case "SetProductStatus":
                        var prodStat = await _context.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == cmd.product.ToLower());
                        if (prodStat == null) return $"- Hata: '{cmd.product}' bulunamadı.";
                        prodStat.IsActive = cmd.status;
                        await _context.SaveChangesAsync();
                        return $"- Başarılı: {cmd.product} {(cmd.status ? "Aktif" : "Pasif")} yapıldı.";

                    case "MarkOrderComplete":
                        var order = await _context.Orders.FindAsync(cmd.orderId);
                        if (order == null) return $"- Hata: Sipariş #{cmd.orderId} bulunamadı.";
                        if (order.OrderState == OrderState.Completed) return $"- Bilgi: Sipariş #{cmd.orderId} zaten tamamlanmış.";

                        order.OrderState = OrderState.Completed;
                        var activePeriod = await _context.WorkPeriods.FirstOrDefaultAsync(x => x.IsActive);
                        if (activePeriod != null) order.WorkPeriodId = activePeriod.Id;

                        await _context.SaveChangesAsync();
                        return $"- Başarılı: Sipariş #{cmd.orderId} tamamlandı.";

                    default: return $"- Hata: Geçersiz komut ({cmd.command}).";
                }
            }
            catch (Exception ex) { return $"- Hata: {ex.Message}"; }
        }

        private async Task<IActionResult> GetFinalAnswerFromAi(string query, string result)
        {
            var sessionHistory = HttpContext.Session.GetString("AiChatHistory");
            var history = string.IsNullOrEmpty(sessionHistory) ? new List<ChatMessage>() : JsonConvert.DeserializeObject<List<ChatMessage>>(sessionHistory);

            var contextPrompt = $"SİSTEM RAPORU: '{query}' için yapılan işlemler:\n{result}\nBuna göre kullanıcıya tek bir özet cümleyle işlemin bittiğini söyle.";

            history.Add(new ChatMessage { Role = "user", Text = contextPrompt });

            var contentsList = history.Select(h => new { role = h.Role, parts = new[] { new { text = h.Text } } }).ToList();
            var jsonPayload = new { contents = contentsList };
            var apiKey = _configuration["Gemini:ApiKey"];

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(jsonPayload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}?key={apiKey}", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(jsonResponse)!;
                string finalAnswer = data.candidates[0].content.parts[0].text;

                history.Add(new ChatMessage { Role = "model", Text = finalAnswer });
                HttpContext.Session.SetString("AiChatHistory", JsonConvert.SerializeObject(history));

                return Ok(new { answer = finalAnswer });
            }
        }

        private async Task<string> GetSystemPrompt()
        {
            var dataSummary = await GetDbSummaryForAgent();
            var sb = new StringBuilder();

            sb.AppendLine("Sen GastroArt restoranının hem **Baş Veri Analisti** hem de **Sistem Operatörüsün**.");
            sb.AppendLine("Aşağıdaki JSON verilerini (Ürünler, Geçmiş Satışlar, Aktif Siparişler) detaylıca incele.");

            sb.AppendLine("\n--- GÖREV TANIMIN ---");
            sb.AppendLine("1. **ANALİST ROLÜ:** Kullanıcı analiz sorusu sorarsa (Örn: 'En çok kar edilen gün?', 'Bugün durumlar nasıl?', 'Hangi ürün satmıyor?'), elindeki veriyi matematiksel olarak analiz et ve yorumlayarak cevapla. Asla 'bilgim yok' deme, veri aşağıda mevcut.");
            sb.AppendLine("2. **OPERATÖR ROLÜ:** Kullanıcı sistemsel bir değişiklik isterse (Örn: 'Siparişi tamamla', 'Stok güncelle'), SADECE aşağıdaki JSON formatında komut üret.");

            sb.AppendLine("\n--- KOMUT FORMATLARI (Sadece Eylem Gerektiğinde Kullan) ---");
            sb.AppendLine("- Stok: {\"command\": \"UpdateStock\", \"product\": \"ÜrünAdı\", \"stock\": 50}");
            sb.AppendLine("- Durum: {\"command\": \"SetProductStatus\", \"product\": \"ÜrünAdı\", \"status\": true/false}");
            sb.AppendLine("- Sipariş: {\"command\": \"MarkOrderComplete\", \"orderId\": 123}");

            sb.AppendLine("\n--- ÇOKLU İŞLEM ---");
            sb.AppendLine("Birden fazla işlem gerekiyorsa JSON dizisi gönder: [ {...}, {...} ]");

            sb.AppendLine("\n--- MEVCUT RESTORAN VERİLERİ ---");
            sb.AppendLine(dataSummary);

            return sb.ToString();
        }

        private async Task<string> GetDbSummaryForAgent()
        {
            var allProducts = await _context.Products
                .Select(p => new { p.Name, p.Stock, p.Price, p.TrackStock, Status = p.IsActive ? "Aktif" : "Pasif" })
                .ToListAsync();

            var salesHistory = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderState == OrderState.Completed)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    Date = o.OrderDate.ToString("yyyy-MM-dd"),
                    Time = o.OrderDate.ToString("HH:mm"),
                    Total = o.TotalAmount,
                    Items = o.OrderItems.Select(i => new { i.ProductName, i.Quantity }).ToList()
                })
                .Take(500)
                .ToListAsync();

            var activeOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderState == OrderState.Pending)
                .Select(o => new { OrderId = o.Id, Table = o.TableNo, Total = o.TotalAmount, Items = o.OrderItems.Select(i => i.ProductName).ToList() })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("--- 1. ÜRÜNLER VE STOKLAR ---");
            sb.AppendLine(JsonConvert.SerializeObject(allProducts));

            sb.AppendLine("\n--- 2. GEÇMİŞ SATIŞLAR ---");
            sb.AppendLine(JsonConvert.SerializeObject(salesHistory));

            sb.AppendLine("\n--- 3. BEKLEYEN SİPARİŞLER ---");
            sb.AppendLine(JsonConvert.SerializeObject(activeOrders));

            return sb.ToString();
        }
    }

    public class AiRequest { public string Message { get; set; } }
    public class AiCommand { public string command { get; set; } public string product { get; set; } public int stock { get; set; } public int orderId { get; set; } public bool status { get; set; } }
    public class ChatMessage { public string Role { get; set; } public string Text { get; set; } }
}
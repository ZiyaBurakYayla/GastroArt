using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace GastroArt.Controllers
{
    public class QrController : Controller
    {
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser")))
                return RedirectToAction("Login", "Admin");

            return View();
        }

        [HttpPost]
        public IActionResult Generate(int startTable, int endTable)
        {
            var qrCodes = new List<QrCodeModel>();
            string baseUrl = $"{this.Request.Scheme}://{this.Request.Host}/Menu/Index";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                for (int i = startTable; i <= endTable; i++)
                {
                    string url = $"{baseUrl}?tableId={i}";
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    string base64Image = Convert.ToBase64String(qrCodeBytes);

                    qrCodes.Add(new QrCodeModel
                    {
                        TableNo = i,
                        QrImage = $"data:image/png;base64,{base64Image}"
                    });
                }
            }

            return View("Print", qrCodes);
        }
    }

    // Yardımcı Model
    public class QrCodeModel
    {
        public int TableNo { get; set; }
        public string QrImage { get; set; }
    }
}
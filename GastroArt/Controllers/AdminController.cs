using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GastroArt.Models;
using GastroArt.Models.Data;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string Username, string Password)
    {
        var user = _context.Admins.FirstOrDefault(x => x.Username == Username && x.Password == Password);

        if (user != null)
        {
            HttpContext.Session.SetString("AdminUser", user.Username);
            return RedirectToAction("Index","Category");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

}
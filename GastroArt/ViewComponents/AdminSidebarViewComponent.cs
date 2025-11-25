using Microsoft.AspNetCore.Mvc;

namespace GastroArt.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

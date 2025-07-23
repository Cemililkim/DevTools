using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class ColorPaletteGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

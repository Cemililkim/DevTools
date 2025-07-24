using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class QRCodeToolsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

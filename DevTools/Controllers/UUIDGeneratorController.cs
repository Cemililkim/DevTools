using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class UUIDGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

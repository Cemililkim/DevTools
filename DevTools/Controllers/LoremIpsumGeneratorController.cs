using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class LoremIpsumGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

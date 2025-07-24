using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class WordCounterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

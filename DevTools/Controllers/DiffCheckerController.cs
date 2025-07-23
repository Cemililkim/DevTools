using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class DiffCheckerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

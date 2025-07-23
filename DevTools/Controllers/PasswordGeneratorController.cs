using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class PasswordGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

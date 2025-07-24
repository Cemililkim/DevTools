using Microsoft.AspNetCore.Mvc;

namespace DevTools.Controllers
{
    public class MarkdownPreviewerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

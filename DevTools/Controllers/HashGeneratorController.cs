using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace DevTools.Controllers
{
    public class HashGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

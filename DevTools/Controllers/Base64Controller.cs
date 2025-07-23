using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace DevTools.Controllers
{
    public class Base64Controller : Controller
    {
        [HttpPost]
        public IActionResult Encode(string input)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var encoded = Convert.ToBase64String(bytes);
                return Json(new { success = true, result = encoded });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Encoding failed: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Decode(string input)
        {
            try
            {
                var bytes = Convert.FromBase64String(input);
                var decoded = Encoding.UTF8.GetString(bytes);
                return Json(new { success = true, result = decoded });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Decoding failed: " + ex.Message });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

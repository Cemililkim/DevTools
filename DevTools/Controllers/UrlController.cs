using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace DevTools.Controllers
{
    public class UrlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Encode(string input)
        {
            try
            {
                var encoded = WebUtility.UrlEncode(input);
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
                var decoded = WebUtility.UrlDecode(input);
                return Json(new { success = true, result = decoded });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Decoding failed: " + ex.Message });
            }
        }
    }
}

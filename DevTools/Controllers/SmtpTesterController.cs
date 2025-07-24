using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace DevTools.Controllers
{
    public class SmtpTesterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendTestEmail(
            string smtpServer, int port, bool enableSsl,
            string username, string password,
            string fromEmail, string toEmail)
        {
            try
            {
                var client = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = "SMTP Test Message",
                    Body = "This is a test email sent using the SMTP Tester tool.",
                    IsBodyHtml = false
                };

                client.Send(message);

                ViewBag.IsSuccess = true;
                ViewBag.Result = $"Test email successfully sent to {toEmail}";
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Result = $"Error: {ex.Message}";
            }

            return View("Index");
        }
    }
}

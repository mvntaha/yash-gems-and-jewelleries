using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Email address is required." });
            }

            // Simple email validation
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    return Json(new { success = false, message = "Invalid email format." });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Invalid email format." });
            }

            // Check for duplicate subscription
            if (await _newsletterService.IsEmailSubscribedAsync(email))
            {
                return Json(new { success = false, message = "This email is already subscribed to our newsletter." });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _newsletterService.SubscribeAsync(email, null, ipAddress, "Footer");

            if (result)
            {
                return Json(new { success = true, message = "Thank you for subscribing to our newsletter!" });
            }

            return Json(new { success = false, message = "An error occurred while processing your subscription. Please try again later." });
        }
    }
}

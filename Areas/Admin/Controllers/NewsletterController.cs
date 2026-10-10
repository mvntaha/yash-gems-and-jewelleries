using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        public async Task<IActionResult> Index()
        {
            var subscriptions = await _newsletterService.GetAllSubscriptionsAsync();
            return View(subscriptions);
        }
    }
}

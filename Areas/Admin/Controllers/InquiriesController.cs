using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yash_Gems___Jewelleries.Services;
using Yash_Gems___Jewelleries.ViewModels;

namespace Yash_Gems___Jewelleries.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InquiriesController : Controller
    {
        private readonly IInquiryService _inquiryService;

        public InquiriesController(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }

        public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1)
        {
            int pageSize = 10;
            var inquiries = await _inquiryService.GetAllInquiriesAsync(page, pageSize, search, isActive);
            var totalCount = await _inquiryService.GetInquiriesCountAsync(search, isActive);

            var viewModel = new InquiryIndexViewModel
            {
                Inquiries = inquiries.Select(i => new InquiryListItemViewModel
                {
                    InquiryId = i.InquiryId,
                    CustomerName = i.FullName,
                    Email = i.Email,
                    CustomerEmail = i.Email,
                    ProfilePictureUrl = i.User?.ProfilePictureUrl,
                    Subject = i.Subject,
                    Date = i.CreatedDate,
                    Status = i.Status,
                    OriginalMessage = i.Message,
                    HasReplied = !string.IsNullOrEmpty(i.AdminResponse)
                }),
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                Search = search,
                IsActive = isActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var success = await _inquiryService.ToggleStatusAsync(id);
            if (success)
            {
                var updatedInquiry = await _inquiryService.GetInquiryByIdAsync(id);
                return Json(new { success = true, message = "Status updated successfully.", type = "success", isActive = (updatedInquiry?.Status != "Closed" && updatedInquiry?.Status != "Deactivated") });
            }
            return Json(new { success = false, message = "Failed to update status." });
        }

        [HttpGet]
        public async Task<IActionResult> GetReplyModal(int id)
        {
            var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
            if (inquiry == null) return NotFound();

            var viewModel = new InquiryReplyViewModel
            {
                InquiryId = inquiry.InquiryId,
                CustomerName = inquiry.FullName,
                Email = inquiry.Email,
                Subject = inquiry.Subject,
                OriginalMessage = inquiry.Message,
                AdminResponse = inquiry.AdminResponse ?? string.Empty,
                Status = inquiry.Status,
                CreatedDate = inquiry.CreatedDate,
                LastUpdatedDate = inquiry.ModifiedDate
            };

            return PartialView("_ReplyPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(InquiryReplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please provide a valid response." });
            }

            var respondedBy = User.Identity?.Name ?? "Admin";
            var success = await _inquiryService.SubmitReplyAsync(model.InquiryId, model.AdminResponse, respondedBy, model.Status);

            if (success)
            {
                return Json(new { success = true, message = "Reply submitted successfully.", type = "success" });
            }

            return Json(new { success = false, message = "Failed to submit reply." });
        }
    }
}

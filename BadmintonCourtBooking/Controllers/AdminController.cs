using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var viewModel = await _adminDashboardService.GetDashboardAsync(cancellationToken);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVenue(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.ApproveVenueAsync(id, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "success" : "error";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVenue(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.RejectVenueAsync(id, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "warning" : "error";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.LockUserAsync(id, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "warning" : "error";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.UnlockUserAsync(id, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "success" : "error";

            return RedirectToAction(nameof(Index));
        }
    }
}

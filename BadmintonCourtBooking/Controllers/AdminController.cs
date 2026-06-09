using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Extensions;
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

        public async Task<IActionResult> Venues(string? selectedVenueId, CancellationToken cancellationToken)
        {
            var viewModel = await _adminDashboardService.GetVenueManagementAsync(selectedVenueId, cancellationToken);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(AdminVenueInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Vui lòng nhập đầy đủ thông tin cụm sân."), "error");
                return RedirectToVenues(model.SelectedVenueId);
            }

            var result = await _adminDashboardService.CreateVenueAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVenue(AdminVenueInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Thông tin cụm sân chưa hợp lệ. Vui lòng kiểm tra lại."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.Id);
            }

            var result = await _adminDashboardService.UpdateVenueAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourt(AdminCourtInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Vui lòng kiểm tra lại thông tin sân con trước khi lưu."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.VenueId);
            }

            var result = await _adminDashboardService.CreateCourtAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.VenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourt(AdminCourtInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Thông tin sân con chưa hợp lệ. Vui lòng kiểm tra lại."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.VenueId);
            }

            var result = await _adminDashboardService.UpdateCourtAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.VenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.ApproveBookingAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");

            return RedirectToAction(nameof(Index), null, null, "bookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.RejectBookingAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "warning" : "error");

            return RedirectToAction(nameof(Index), null, null, "bookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVenue(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.ApproveVenueAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVenue(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.RejectVenueAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "warning" : "error");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.LockUserAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "warning" : "error");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.UnlockUserAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");

            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectToVenues(string? selectedVenueId)
        {
            return string.IsNullOrWhiteSpace(selectedVenueId)
                ? RedirectToAction(nameof(Venues))
                : RedirectToAction(nameof(Venues), new { selectedVenueId });
        }

        private void SetToast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }
    }
}

using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Extensions;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    [Authorize(Roles = AppRoles.Owner)]
    public class OwnerController : Controller
    {
        private readonly IOwnerDashboardService _ownerDashboardService;

        public OwnerController(IOwnerDashboardService ownerDashboardService)
        {
            _ownerDashboardService = ownerDashboardService;
        }

        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var viewModel = await _ownerDashboardService.GetDashboardAsync(cancellationToken);
            return View(viewModel);
        }

        public async Task<IActionResult> Venues(string? selectedVenueId, CancellationToken cancellationToken)
        {
            var viewModel = await _ownerDashboardService.GetVenueManagementAsync(selectedVenueId, cancellationToken);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(OwnerVenueInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Vui lòng nhập đầy đủ thông tin cụm sân trước khi gửi duyệt."), "error");
                return RedirectToVenues(model.SelectedVenueId);
            }

            var result = await _ownerDashboardService.CreateVenueAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVenue(OwnerVenueInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Thông tin cụm sân chưa hợp lệ. Vui lòng kiểm tra lại."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.Id);
            }

            var result = await _ownerDashboardService.UpdateVenueAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourt(OwnerCourtInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Vui lòng kiểm tra lại thông tin sân con trước khi lưu."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.VenueId);
            }

            var result = await _ownerDashboardService.CreateCourtAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.VenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourt(OwnerCourtInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetToast(ModelState.GetFirstErrorMessage("Thông tin sân con chưa hợp lệ. Vui lòng kiểm tra lại."), "error");
                return RedirectToVenues(model.SelectedVenueId ?? model.VenueId);
            }

            var result = await _ownerDashboardService.UpdateCourtAsync(model, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");
            return RedirectToVenues(result.Data ?? model.SelectedVenueId ?? model.VenueId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(string id, CancellationToken cancellationToken)
        {
            var result = await _ownerDashboardService.ApproveBookingAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "success" : "error");

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(string id, CancellationToken cancellationToken)
        {
            var result = await _ownerDashboardService.RejectBookingAsync(id, cancellationToken);
            SetToast(result.Message, result.Succeeded ? "warning" : "error");

            return RedirectToAction(nameof(Dashboard));
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

using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Extensions;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class VenueController : Controller
    {
        private readonly IVenueCatalogService _venueCatalogService;
        private readonly IBookingService _bookingService;

        public VenueController(IVenueCatalogService venueCatalogService, IBookingService bookingService)
        {
            _venueCatalogService = venueCatalogService;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index([FromQuery] VenueSearchCriteria filters, CancellationToken cancellationToken)
        {
            filters.Normalize();

            var viewModel = new VenueIndexViewModel
            {
                Filters = filters,
                DistrictOptions = await _venueCatalogService.GetApprovedDistrictsAsync(cancellationToken),
                Venues = await _venueCatalogService.SearchApprovedVenuesAsync(filters, cancellationToken)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(string id, CancellationToken cancellationToken)
        {
            var viewModel = await _venueCatalogService.GetVenueDetailAsync(id, cancellationToken);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // AJAX API to get slots for a specific date
        [HttpGet]
        public async Task<IActionResult> GetSlots(string id, string dateKey, CancellationToken cancellationToken)
        {
            var slotMatrix = await _venueCatalogService.GetSlotMatrixAsync(id, dateKey, cancellationToken);
            if (slotMatrix == null)
            {
                return NotFound();
            }
            
            // Format slot matrix into a simpler structure for JSON response
            var response = slotMatrix.ToDictionary(
                courtEntry => courtEntry.Key,
                courtEntry => courtEntry.Value.ToDictionary(
                    slotEntry => slotEntry.Key,
                    slotEntry => slotEntry.Value.ToString() // "Available", "Booked", "Pending"
                )
            );

            return Json(response);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Player)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSlot(CreateBookingInputModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = ModelState.GetFirstErrorMessage("Vui lòng chọn lại ngày và khung giờ muốn đặt.");
                TempData["ToastType"] = "error";
                return RedirectToVenueDetail(model.VenueId);
            }

            var result = await _bookingService.CreateBookingAsync(model, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "success" : "error";

            return result.Succeeded
                ? RedirectToAction("Index", "Booking")
                : RedirectToVenueDetail(model.VenueId);
        }

        private IActionResult RedirectToVenueDetail(string? venueId)
        {
            return string.IsNullOrWhiteSpace(venueId)
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(Detail), new { id = venueId });
        }
    }
}

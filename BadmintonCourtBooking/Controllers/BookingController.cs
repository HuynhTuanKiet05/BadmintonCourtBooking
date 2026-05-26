using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    [Authorize(Roles = AppRoles.Player)]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetPlayerBookingsAsync();
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "success" : "error";

            return RedirectToAction(nameof(Index));
        }
    }
}

using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            var bookings = MockData.MyBookings;
            return View(bookings);
        }

        [HttpPost]
        public IActionResult Cancel(string id)
        {
            var booking = MockData.MyBookings.FirstOrDefault(b => b.Id == id);
            if (booking != null)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.When = "cancelled";
                TempData["ToastMessage"] = $"Đã hủy lịch đặt tại {booking.Venue} ({booking.Court}) thành công.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Không tìm thấy thông tin lịch đặt sân.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class OwnerController : Controller
    {
        public IActionResult Dashboard()
        {
            var viewModel = MockData.GetDashboardViewModel();
            return View(viewModel);
        }

        public IActionResult Venues()
        {
            var venues = MockData.Venues;
            return View(venues);
        }

        [HttpPost]
        public IActionResult ApproveBooking(string id)
        {
            var ownerBooking = MockData.OwnerBookings.FirstOrDefault(ob => ob.Id == id);
            if (ownerBooking != null)
            {
                ownerBooking.Status = BookingStatus.Confirmed;
                
                // Also update the corresponding customer booking if exists
                // ob1 corresponds to Nguyễn Minh Khoa, ob2 Trần Phương Linh, ob3 Lê Quốc Bảo, ob4 Phạm Hồng Nhung, ob5 Đặng Tuấn Anh
                // Since this is mock data, we match by customer name or court/time
                var customerBooking = MockData.MyBookings.FirstOrDefault(mb => 
                    mb.Court == ownerBooking.Court && 
                    ownerBooking.Time.Contains(mb.Time.Split(" – ")[0]));
                if (customerBooking != null)
                {
                    customerBooking.Status = BookingStatus.Confirmed;
                }

                TempData["ToastMessage"] = $"Đã duyệt lịch đặt sân thành công cho {ownerBooking.Customer}.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Không tìm thấy thông tin lịch đặt sân.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public IActionResult RejectBooking(string id)
        {
            var ownerBooking = MockData.OwnerBookings.FirstOrDefault(ob => ob.Id == id);
            if (ownerBooking != null)
            {
                ownerBooking.Status = BookingStatus.Cancelled;

                var customerBooking = MockData.MyBookings.FirstOrDefault(mb => 
                    mb.Court == ownerBooking.Court && 
                    ownerBooking.Time.Contains(mb.Time.Split(" – ")[0]));
                if (customerBooking != null)
                {
                    customerBooking.Status = BookingStatus.Cancelled;
                    customerBooking.When = "cancelled";
                }

                TempData["ToastMessage"] = $"Đã từ chối lịch đặt sân của {ownerBooking.Customer}.";
                TempData["ToastType"] = "warning";
            }
            else
            {
                TempData["ToastMessage"] = "Không tìm thấy thông tin lịch đặt sân.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}

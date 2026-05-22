using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = MockData.GetAdminViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ApproveVenue(string id)
        {
            var pending = MockData.PendingVenues.FirstOrDefault(pv => pv.Id == id);
            if (pending != null)
            {
                // Remove from pending
                MockData.PendingVenues.Remove(pending);

                // Create new active Venue and add to active list
                var newVenue = new Venue
                {
                    Id = pending.Id, // Keep pv1, pv2, pv3 as id
                    Name = pending.Name,
                    District = pending.District,
                    Address = $"Số 1 Đường số 2, {pending.District}",
                    OpenHours = "06:00 – 22:00",
                    PriceFrom = 90_000,
                    Rating = 5.0,
                    Reviews = 1,
                    ResponseFast = true,
                    HasSlotsToday = true,
                    Highlight = "Sân mới duyệt - Ưu đãi lớn",
                    Description = $"Cụm sân cầu lông tiêu chuẩn quốc tế được quản lý bởi {pending.Owner}. Không gian thoáng đãng, hệ thống chiếu sáng chuẩn thi đấu, sàn thảm PVC chất lượng cao.",
                    Courts = new List<Court>()
                };

                // Generate sub-courts
                for (int i = 1; i <= pending.Courts; i++)
                {
                    newVenue.Courts.Add(new Court
                    {
                        Id = $"c{i}",
                        Name = $"Sân {i}",
                        PricePerHour = 90_000,
                        Note = i == 1 ? "Sân VIP" : ""
                    });
                }

                MockData.Venues.Add(newVenue);

                TempData["ToastMessage"] = $"Đã duyệt hồ sơ đăng ký của '{pending.Name}' thành công. Sân đã được đưa lên hệ thống!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Không tìm thấy hồ sơ yêu cầu đăng ký sân.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RejectVenue(string id)
        {
            var pending = MockData.PendingVenues.FirstOrDefault(pv => pv.Id == id);
            if (pending != null)
            {
                MockData.PendingVenues.Remove(pending);

                TempData["ToastMessage"] = $"Đã từ chối và lưu trữ hồ sơ đăng ký của '{pending.Name}' (Chủ sân: {pending.Owner}).";
                TempData["ToastType"] = "warning";
            }
            else
            {
                TempData["ToastMessage"] = "Không tìm thấy hồ sơ yêu cầu đăng ký sân.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

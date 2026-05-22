using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class VenueController : Controller
    {
        public IActionResult Index()
        {
            var venues = MockData.Venues;
            return View(venues);
        }

        public IActionResult Detail(string id)
        {
            var venue = MockData.Venues.FirstOrDefault(v => v.Id == id);
            if (venue == null)
            {
                return NotFound();
            }

            // Prepare date options (next 7 days starting today)
            var dateOptions = new List<string>();
            var today = DateTime.Today;
            
            // Format in Vietnamese style, e.g. "Hôm nay · 20/05", "Thứ 5 · 21/05"
            string[] viDays = { "Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
            
            for (int i = 0; i < 7; i++)
            {
                var targetDate = today.AddDays(i);
                string dateStr;
                if (i == 0)
                {
                    dateStr = $"Hôm nay · {targetDate:dd/MM}";
                }
                else if (i == 1)
                {
                    dateStr = $"Ngày mai · {targetDate:dd/MM}";
                }
                else
                {
                    string dayName = viDays[(int)targetDate.DayOfWeek];
                    dateStr = $"{dayName} · {targetDate:dd/MM}";
                }
                dateOptions.Add(dateStr);
            }

            // Default to today's date key for slot matrix
            string defaultDateKey = dateOptions[0];
            var slotMatrix = MockData.BuildSlotMatrix(venue, defaultDateKey);

            var viewModel = new VenueDetailViewModel
            {
                Venue = venue,
                SlotMatrix = slotMatrix,
                TimeSlots = MockData.TimeSlots,
                DateOptions = dateOptions
            };

            return View(viewModel);
        }

        // AJAX API to get slots for a specific date
        [HttpGet]
        public IActionResult GetSlots(string id, string dateKey)
        {
            var venue = MockData.Venues.FirstOrDefault(v => v.Id == id);
            if (venue == null)
            {
                return NotFound();
            }

            var slotMatrix = MockData.BuildSlotMatrix(venue, dateKey);
            
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
    }
}

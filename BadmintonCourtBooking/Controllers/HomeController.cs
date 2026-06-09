using System.Diagnostics;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVenueCatalogService _venueCatalogService;

        public HomeController(ILogger<HomeController> logger, IVenueCatalogService venueCatalogService)
        {
            _logger = logger;
            _venueCatalogService = venueCatalogService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredVenues = await _venueCatalogService.GetFeaturedVenuesAsync();
            var heroVenue = featuredVenues.FirstOrDefault();
            
            if (heroVenue is not null)
            {
                var displayDate = DateTime.Today;
                if (DateTime.Now.Hour >= 20)
                {
                    displayDate = DateTime.Today.AddDays(1);
                }
                
                var dateKey = PresentationFormatter.FormatDateChip(displayDate);
                var slotMatrix = await _venueCatalogService.GetSlotMatrixAsync(heroVenue.Id, dateKey);
                ViewBag.HeroSlotMatrix = slotMatrix;
            }
            
            return View(featuredVenues);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

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

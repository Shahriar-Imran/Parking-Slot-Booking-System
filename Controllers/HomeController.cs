using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Models;

namespace ParkingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // =========================
        // HOME PAGE
        // =========================
        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // ABOUT PAGE
        // =========================
        public IActionResult About()
        {
            return View();
        }

        // =========================
        // CONTACT US PAGE
        // =========================
        public IActionResult Contact()
        {
            return View();
        }

        // =========================
        // PRIVACY PAGE
        // =========================
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

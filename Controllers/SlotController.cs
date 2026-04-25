using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using ParkingSystem.ViewModels;

namespace ParkingSystem.Controllers
{
    public class SlotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlotController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Availability Page
        // =========================
        public IActionResult Availability()
        {
            var model = new SlotAvailabilityViewModel
            {
                Areas = _context.ParkingAreas.ToList(),
                Date = DateTime.Now,
                DurationHours = 1
            };

            return View(model);
        }


        // =========================
        // POST: Search Availability
        // =========================
        [HttpPost]
        public async Task<IActionResult> Availability(SlotAvailabilityViewModel model)
        {
            model.Areas = _context.ParkingAreas.ToList();

            if (model.DurationHours <= 0)
            {
                ModelState.AddModelError("", "Invalid duration");
                return View(model);
            }

            var start = model.Date;
            var end = start.AddHours(model.DurationHours);

            var bookedSlotIds = _context.BookingSlots
                .Where(b => b.Booking.StartTime < end && b.Booking.EndTime > start)
                .Select(b => b.SlotId);

            
            var slots = await _context.ParkingSlots
                .Include(s => s.ParkingArea)               
                .ToListAsync();

            var bookedIds = bookedSlotIds.ToList();
            ViewBag.BookedSlots = bookedIds;

            model.AvailableSlots = slots;
            ViewBag.Date = model.Date;

            return View(model);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Availability()
        {
            var model = new SlotAvailabilityViewModel
            {
                Areas = await _context.ParkingAreas.ToListAsync(),
                AvailableSlots = new(),
                Date = DateTime.Now,
                DurationHours = 1,
                NumberOfSlots = 1
            };

            return View(model);
        }

        // =========================
        // POST: Search Availability
        // =========================
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Availability(SlotAvailabilityViewModel model)
        {
            // Convert selected slots from string → list
            var selectedSlotIds = model.SelectedSlots?.Split(',')
                                    .Select(int.Parse)
                                    .ToList();

            // Load areas again (important for dropdown)
            model.Areas = await _context.ParkingAreas.ToListAsync();

            var requestedStart = model.Date;
            var requestedEnd = model.Date.AddHours(model.DurationHours);

            // Find already booked slots
            var bookedSlotIds = _context.BookingSlots
                .Where(bs =>
                    bs.Booking.StartTime < requestedEnd &&
                    bs.Booking.EndTime > requestedStart)
                .Select(bs => bs.SlotId);

            // Get available slots
            var availableSlots = await _context.ParkingSlots
                .Where(s => s.AreaId == model.AreaId && !bookedSlotIds.Contains(s.SlotId))
                .ToListAsync();

            model.AvailableSlots = availableSlots;

            // Calculate price
            if (availableSlots.Any())
            {
                model.HourlyRate = availableSlots.First().HourlyRate;
                model.TotalAmount = model.HourlyRate * model.DurationHours * model.NumberOfSlots;
            }

            // OPTIONAL: You can use selectedSlotIds later for booking

            return View(model);
        }
    }
}
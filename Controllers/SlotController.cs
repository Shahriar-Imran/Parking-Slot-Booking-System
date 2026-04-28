using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using ParkingSystem.ViewModels;
using ParkingSystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            // 🔥 REMOVE EXPIRED LOCKS
            var expiredLocks = _context.SlotLocks
                .Where(l => l.ExpireTime < DateTime.Now);

            _context.SlotLocks.RemoveRange(expiredLocks);
            _context.SaveChanges();

            model.Areas = _context.ParkingAreas.ToList();

            if (model.DurationHours <= 0)
            {
                ModelState.AddModelError("", "Invalid duration");
                return View(model);
            }

            var start = model.Date;
            var end = start.AddHours(model.DurationHours);

            // 🔥 BOOKED
            var bookedIds = _context.BookingSlots
                .Where(b => !b.Booking.IsCancelled &&
                            b.Booking.StartTime < end &&
                            b.Booking.EndTime > start)
                .Select(b => b.SlotId)
                .ToList();

            // 🔥 LOCKED (FIXED)
            var now = DateTime.Now;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lockedIds = _context.SlotLocks
                .Where(l => l.ExpireTime > now
                    && l.UserId != userId   // 🔥 IMPORTANT
                    && l.StartTime < end
                    && l.EndTime > start)
                .Select(l => l.SlotId)
                .ToList();

            // 🔥 MERGE (FIXED)
            var unavailableSlots = bookedIds
                .Union(lockedIds)
                .ToList();

            // 🔥 ALL SLOTS
            var slots = await _context.ParkingSlots
                .Include(s => s.ParkingArea)
                .ToListAsync();

            var locks = _context.SlotLocks
                .Where(l => l.ExpireTime > DateTime.Now)
                .Select(l => new {
                    slotId = l.SlotId,        // 🔥 force lowercase
                    expireTime = l.ExpireTime
                })
                .ToList();

            ViewBag.LockTimes = locks;
            ViewBag.BookedSlots = unavailableSlots;

            model.AvailableSlots = slots;
            ViewBag.Date = model.Date;

            return View(model);
        }

        //======== Lock Slot =========
        [Authorize]
        [HttpPost]
        public IActionResult LockSlot([FromBody] LockRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;

            var start = model.StartTime;
            var end = start.AddHours(model.Duration);

            // 🔥 CHECK OVERLAP WITH OTHER USERS
            var exists = _context.SlotLocks
                .Any(l => l.SlotId == model.SlotId
                       && l.ExpireTime > now
                       && l.UserId != userId
                       && l.StartTime < end
                       && l.EndTime > start);

            if (exists)
                return BadRequest();

            // 🔥 CHECK SAME USER
            var alreadyMine = _context.SlotLocks
                .FirstOrDefault(l => l.SlotId == model.SlotId && l.UserId == userId);

            if (alreadyMine != null)
                return Ok();

            _context.SlotLocks.Add(new SlotLock
            {
                SlotId = model.SlotId,
                UserId = userId,
                StartTime = start,
                EndTime = end,
                ExpireTime = now.AddMinutes(3)
            });

            _context.SaveChanges();

            return Ok();
        }
    }
}
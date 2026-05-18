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
using Microsoft.AspNetCore.SignalR;

namespace ParkingSystem.Controllers
{
    public class SlotController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SlotHub> _hub;

        public SlotController(ApplicationDbContext context, IHubContext<SlotHub> hub)
        {
            _context = context;
            _hub = hub; // 🔥 ASSIGN
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

            var expiredLocks = _context.SlotLocks
                .Where(l => l.ExpireTime < DateTime.Now)
                .ToList();

            _context.SlotLocks.RemoveRange(expiredLocks);
            await _context.SaveChangesAsync();

            foreach (var lockItem in expiredLocks)
            {
                var slot = _context.ParkingSlots
                    .Include(s => s.ParkingArea)
                    .First(s => s.SlotId == lockItem.SlotId);

                var areaKey = $"{slot.ParkingArea.BlockNumber}-{slot.ParkingArea.VehicleType}";

                await _hub.Clients.All.SendAsync("ReceiveSlotUpdate", new
                {
                    slotId = lockItem.SlotId,
                    status = "available",
                    userId = lockItem.UserId,
                    area = areaKey
                });
            }

            model.Areas = _context.ParkingAreas.ToList();

            if (model.DurationHours <= 0)
            {
                ModelState.AddModelError("", "Invalid duration");
                return View(model);
            }

            var start = model.Date;
            var end = start.AddHours(model.DurationHours);

            start = start.AddSeconds(-start.Second).AddMilliseconds(-start.Millisecond);
            end = end.AddSeconds(-end.Second).AddMilliseconds(-end.Millisecond);

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
                    && l.UserId != userId   
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
                    slotId = l.SlotId,        
                    expireTime = l.ExpireTime,
                    userId = l.UserId
                })
                .ToList();

            ViewBag.LockTimes = locks;
            ViewBag.BookedSlots = unavailableSlots;
            ViewBag.CurrentUserId = userId;

            model.AvailableSlots = slots;
            ViewBag.Date = model.Date;

            return View(model);
        }

        // =========================
        // LOCK SLOT
        // =========================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LockSlot([FromBody] LockRequest model)
        {

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM SlotLocks WHERE ExpireTime < GETDATE()");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;

            var start = model.StartTime;
            var end = start.AddHours(model.Duration);

            start = start.AddSeconds(-start.Second).AddMilliseconds(-start.Millisecond);
            end = end.AddSeconds(-end.Second).AddMilliseconds(-end.Millisecond);

            var slot = _context.ParkingSlots
                .Include(s => s.ParkingArea)
                .First(s => s.SlotId == model.SlotId);

            var areaKey = $"{slot.ParkingArea.BlockNumber}-{slot.ParkingArea.VehicleType}";

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var overlappingLocks = _context.SlotLocks
                        .FromSqlRaw(@"
                            SELECT * FROM SlotLocks WITH (UPDLOCK, HOLDLOCK)
                            WHERE SlotId = {0}
                            AND ExpireTime > GETDATE()
                            AND StartTime < {2}
                            AND EndTime > {1}
                        ", model.SlotId, start, end)
                        .ToList();

                    if (overlappingLocks.Any(l => l.UserId != userId))
                    {
                        return BadRequest("Slot already selected by another user!");
                    }

                    _context.SlotLocks.Add(new SlotLock
                    {
                        SlotId = model.SlotId,
                        UserId = userId,
                        StartTime = start,
                        EndTime = end,
                        ExpireTime = DateTime.Now.AddMinutes(1)
                    });

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await _hub.Clients.All.SendAsync("ReceiveSlotUpdate", new
                    {
                        slotId = model.SlotId,
                        status = "locked",
                        userId = userId,
                        area = areaKey
                    });

                    return Ok();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return BadRequest();
                }
            }
        }

        // =========================
        // CHECK MY LOCKS
        // =========================
        [HttpGet]
        public IActionResult CheckMyLocks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var now = DateTime.Now;

            var hasActiveLocks = _context.SlotLocks
                .Any(l => l.UserId == userId && l.ExpireTime > now);

            return Json(new { active = hasActiveLocks });
        }
    }
}
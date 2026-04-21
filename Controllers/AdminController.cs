using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ParkingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ParkingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(),
                TotalBookings = _context.Bookings.Count(),
                TotalSlots = _context.ParkingSlots.Count(),
                TotalRevenue = _context.Payments.Sum(p => p.Amount)
            };

            return View(model);
        }
        public async Task<IActionResult> Users()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // Make user admin
        [HttpPost]
        public async Task<IActionResult> MakeAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction("Users");
        }

        // Manage Slots
        public IActionResult Slots()
        {
            var slots = _context.ParkingSlots
                                .Include(s => s.ParkingArea) // VERY IMPORTANT
                                .ToList();

            return View(slots);
        }

        // ================== Create Area ====================
        // =======================
        // GET: Create Area Page
        // =======================
        public IActionResult CreateArea()
        {
            return View();
        }

        // =======================
        // POST: Create Area
        // =======================
        [HttpPost]
        public async Task<IActionResult> CreateArea(ParkingArea area)
        {
            // Trim first
            area.BlockNumber = area.BlockNumber?.Trim();

            // Validate empty
            if (string.IsNullOrEmpty(area.BlockNumber))
            {
                ModelState.AddModelError("", "Block is required");
                return View(area);
            }

            // Check duplicate
            bool exists = _context.ParkingAreas
                .Any(a => a.BlockNumber == area.BlockNumber &&
                          a.VehicleType == area.VehicleType);

            if (exists)
            {
                ModelState.AddModelError("", "This area already exists!");
                return View(area);
            }

            if (!ModelState.IsValid)
                return View(area);

            _context.ParkingAreas.Add(area);
            await _context.SaveChangesAsync();

            return RedirectToAction("Areas");
        }

        // =======================
        // Edit Area
        // =======================
        public IActionResult EditArea(int id)
        {
            var area = _context.ParkingAreas.Find(id);

            if (area == null)
                return NotFound();

            return View(area);
        }

        [HttpPost]
        public async Task<IActionResult> EditArea(ParkingArea area)
        {
            area.BlockNumber = area.BlockNumber?.Trim();

            if (string.IsNullOrEmpty(area.BlockNumber))
            {
                ModelState.AddModelError("", "Block is required");
                return View(area);
            }

            // 🔥 Duplicate check
            var exists = _context.ParkingAreas
                .Any(a => a.AreaId != area.AreaId &&
                          a.BlockNumber == area.BlockNumber &&
                          a.VehicleType == area.VehicleType);

            if (exists)
            {
                // 👉 Send message to View
                ViewBag.ErrorMessage = "This area already exists!";
                return View(area);
            }

            _context.ParkingAreas.Update(area);
            await _context.SaveChangesAsync();

            return RedirectToAction("Areas");
        }

        // View areas
        public IActionResult Areas()
        {
            var areas = _context.ParkingAreas.ToList();
            return View(areas);
        }

        // =======================
        // Delete Area
        // =======================
        public async Task<IActionResult> DeleteArea(int id)
        {
            var area = await _context.ParkingAreas.FindAsync(id);

            if (area != null)
            {
                _context.ParkingAreas.Remove(area);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Areas");
        }

        // ========================= Create Slot =========================
        public IActionResult CreateSlot()
        {
            ViewBag.Areas = _context.ParkingAreas.ToList();  // Load areas
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSlot(int AreaId)
        {
            if (AreaId == 0)
            {
                ModelState.AddModelError("", "Please select an area");
                ViewBag.Areas = _context.ParkingAreas.ToList();
                return View();
            }

            var area = _context.ParkingAreas.FirstOrDefault(a => a.AreaId == AreaId);

            int count = _context.ParkingSlots.Count(s => s.AreaId == AreaId);

            string slotNumber = $"{area.BlockNumber}-{count + 1}";

            var slot = new ParkingSlot
            {
                AreaId = AreaId,
                SlotNumber = slotNumber
            };

            _context.ParkingSlots.Add(slot);
            await _context.SaveChangesAsync();

            return RedirectToAction("Slots");
        }

        // Edit Slot
        public async Task<IActionResult> EditSlot(int id)
        {
            var slot = await _context.ParkingSlots.FindAsync(id);
            return View(slot);
        }

        [HttpPost]
        public async Task<IActionResult> EditSlot(ParkingSlot slot)
        {
            _context.ParkingSlots.Update(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("Slots");
        }
        // Delete Slot
        [HttpPost]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.ParkingSlots.FindAsync(id);

            if (slot != null)
            {
                _context.ParkingSlots.Remove(slot);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Slots");
        }

        // Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                // Prevent self delete
                if (user.Id == _userManager.GetUserId(User))
                    return RedirectToAction("Users");

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Users");
        }
    }
}

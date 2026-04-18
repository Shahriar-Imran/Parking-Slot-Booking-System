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
        // POST: Save Area
        // =======================
        [HttpPost]
        public async Task<IActionResult> CreateArea(ParkingArea area)
        {
            //Console.WriteLine("Create are hit");
            if (!ModelState.IsValid)
            {
                //Console.WriteLine("Invalid");

                /*foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {state.Key} → Error: {error.ErrorMessage}");
                    }
                }*/

                return View(area);
            }

            _context.ParkingAreas.Add(area);
            await _context.SaveChangesAsync();

            return RedirectToAction("Areas");
        }

        // View areas
        public IActionResult Areas()
        {
            var areas = _context.ParkingAreas.ToList();
            return View(areas);
        }

        // ========================= Create Slot =========================
        public IActionResult CreateSlot()
        {
            ViewBag.Areas = _context.ParkingAreas.ToList();  // Load areas
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSlot(ParkingSlot slot)
        {
            Console.WriteLine("Create Slot HIT");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model Invalid");

                // Reload dropdown if validation fails
                ViewBag.Areas = _context.ParkingAreas.ToList();

                return View(slot);
            }

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

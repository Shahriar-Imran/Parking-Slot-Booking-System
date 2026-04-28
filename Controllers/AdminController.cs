using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ParkingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace ParkingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string MAIN_ADMIN_EMAIL = "admin@gmail.com";

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

                // 🔥 FIXED
                TotalRevenue = _context.Bookings
                    .Where(b => !b.IsCancelled)
                    .Sum(b => b.TotalAmount - b.RefundPreview?? 0)
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
            var currentUser = await _userManager.GetUserAsync(User);

            // ❌ Only main admin can assign admin role
            if (currentUser.Email != MAIN_ADMIN_EMAIL)
            {
                TempData["Error"] = "Only main admin can assign admin role!";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
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
            var currentUser = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction("Users");

            // ❌ Prevent deleting main admin
            if (user.Email == MAIN_ADMIN_EMAIL)
            {
                TempData["Error"] = "Main admin cannot be deleted!";
                return RedirectToAction("Users");
            }

            var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isCurrentMainAdmin = currentUser.Email == MAIN_ADMIN_EMAIL;

            // ❌ Only main admin can delete admins
            if (isTargetAdmin && !isCurrentMainAdmin)
            {
                TempData["Error"] = "Only main admin can delete admin users!";
                return RedirectToAction("Users");
            }

            // ❌ Prevent self delete
            if (user.Id == currentUser.Id)
            {
                TempData["Error"] = "You cannot delete yourself!";
                return RedirectToAction("Users");
            }

            await _userManager.DeleteAsync(user);

            TempData["Success"] = "User deleted successfully";
            return RedirectToAction("Users");
        }

        //Booking history
        public IActionResult BookingHistory()
        {
            var bookings = _context.Bookings
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.Slot)
                        .ThenInclude(s => s.ParkingArea)
                .Include(b => b.User)   // if navigation exists
                .OrderByDescending(b => b.StartTime)
                .ToList();

            return View(bookings);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            // ❌ prevent cancelling completed booking
            if (booking.EndTime <= DateTime.Now)
            {
                TempData["Error"] = "Cannot cancel completed booking!";
                return RedirectToAction("BookingHistory");
            }

            // ❌ prevent duplicate cancel
            if (booking.IsCancelled)
            {
                TempData["Error"] = "Booking already cancelled!";
                return RedirectToAction("BookingHistory");
            }

            // ✅ FULL REFUND
            booking.IsCancelled = true;
            booking.RefundAmount = booking.TotalAmount;
            booking.CancelledAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ SEND EMAIL
            await SendAdminCancellationEmail(booking);

            TempData["Success"] = "Booking cancelled and full refund initiated.";

            return RedirectToAction("BookingHistory");
        }
        

    private async Task SendAdminCancellationEmail(Booking booking)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == booking.UserId);

            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            string fromEmail = "shahriarimran2002@gmail.com";
            string appPassword = "nhnjcthqsdgmqlqv";

            string subject = "Booking Cancelled by Admin";

            string body = $@"
            <h3>Booking Cancelled by Admin</h3>
            <p>Booking ID: <b>{booking.BookingId}</b></p>
            <p>Total Amount: <b>৳ {booking.TotalAmount}</b></p>
            <p>Refund Amount: <b>৳ {booking.TotalAmount}</b></p>
            <p>Your booking has been cancelled by admin.</p>
            <p>The full refund will be processed within 24 hours.</p>
        ";

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(user.Email);

            await smtp.SendMailAsync(mail);
        }

        //========= View User Profile ========//
        public async Task<IActionResult> UserProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser.Email != "admin@gmail.com")
                return RedirectToAction("Users");

            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            return RedirectToAction("Users");
        }
    }




}

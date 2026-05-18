using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ParkingSystem.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

using static QRCoder.PayloadGenerator;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // =========================
        // ADMIN DASHBOARD VIEW
        // =========================

        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(),
                TotalBookings = _context.Bookings.Count(),
                TotalSlots = _context.ParkingSlots.Count(),

                // 🔥 FIXED MATHEMATICAL NULL COERCION
                TotalRevenue = _context.Bookings.Sum(b => b.TotalAmount)
                             - _context.Bookings.Where(b => b.RefundStatus == "Success" || (b.IsCancelled && b.RefundAmount > 0))
                                               .Sum(b => b.RefundAmount ?? b.RefundPreview ?? 0),
                
                RecentBookings = _context.Bookings
                                    .Include(b => b.User)
                                    .OrderByDescending(b => b.CreatedAt)
                                    .Take(5)
                                    .ToList()
            };

            return View(model);
        }

        // =========================
        // USER MANAGEMENT VIEW
        // =========================
        public async Task<IActionResult> Users()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // =========================
        // MAKE USER ADMIN
        // =========================
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

        // =========================
        // MANAGE SLOTS VIEW
        // =========================
        public IActionResult Slots()
        {
            var slots = _context.ParkingSlots
                                .Include(s => s.ParkingArea)
                                .ToList();

            return View(slots);
        }

        // =======================
        // CREATE AREA VIEW
        // =======================
        public IActionResult CreateArea()
        {
            return View();
        }

        // =======================
        // POST: CREATE AREA
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
        //  EDIT AREA VIEW
        // =======================
        public IActionResult EditArea(int id)
        {
            var area = _context.ParkingAreas.Find(id);

            if (area == null)
                return NotFound();

            return View(area);
        }

        // =======================
        //  POST: EDIT AREA VIEW
        // =======================
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

        // =======================
        //  VIEW AREAS
        // =======================
        public IActionResult Areas()
        {
            var areas = _context.ParkingAreas.ToList();
            return View(areas);
        }

        // =======================
        // DELETE AREA
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

        // =======================
        // CREATE SLOT
        // =======================
        public IActionResult CreateSlot()
        {
            ViewBag.Areas = _context.ParkingAreas.ToList();  // Load areas
            return View();
        }

        // =======================
        // POST: CREATE SLOT
        // =======================
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

        // =======================
        // EDIT SLOT
        // =======================
        public async Task<IActionResult> EditSlot(int id)
        {
            var slot = await _context.ParkingSlots.FindAsync(id);
            return View(slot);
        }

        // =======================
        // POST: EDIT SLOT
        // =======================
        [HttpPost]
        public async Task<IActionResult> EditSlot(ParkingSlot slot)
        {
            _context.ParkingSlots.Update(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("Slots");
        }
        // =======================
        // DELETE SLOT
        // =======================
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

        // =======================
        // DELETE USER
        // =======================
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


        // =============================
        // GLOBAL BOOKING HISTORY VIEW
        // =============================
        public IActionResult BookingHistory(string filter = "all", string sort = "created_desc")
        {
            var query = _context.Bookings
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.Slot)
                        .ThenInclude(s => s.ParkingArea)
                .Include(b => b.User)
                .AsQueryable();

            // 1) FILTERING logic
            if (filter == "refunded")
            {
                query = query.Where(b => b.RefundStatus == "Success" || (b.IsCancelled && b.RefundAmount > 0));
            }
            else if (filter == "cancellation_pending")
            {
                query = query.Where(b => b.RefundStatus == "Pending");
            }
            else if (filter == "expired")
            {
                query = query.Where(b => !b.IsCancelled && b.EndTime <= DateTime.Now);
            }

            // 2) SORTING logic
            switch (sort)
            {
                case "created_asc":
                    query = query.OrderBy(b => b.CreatedAt);
                    break;
                case "start_asc":
                    query = query.OrderBy(b => b.StartTime);
                    break;
                case "start_desc":
                    query = query.OrderByDescending(b => b.StartTime);
                    break;
                case "end_asc":
                    query = query.OrderBy(b => b.EndTime);
                    break;
                case "end_desc":
                    query = query.OrderByDescending(b => b.EndTime);
                    break;
                case "created_desc":
                default:
                    query = query.OrderByDescending(b => b.CreatedAt);
                    break;
            }

            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentSort = sort;

            var bookings = query.ToList();
            return View(bookings);
        }

        // =============================
        // CANCEL BOOKING (ADMIN)
        // =============================
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

        // =============================
        // SENDING ADMIN CANCELLATION EMAIL
        // =============================
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

        // =============================
        // VIEW USER PROFILE (ADMIN)
        // =============================
        public async Task<IActionResult> UserProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // =============================
        // REMOVE USER FROM ADMIN ROLE
        // =============================
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

        // =============================
        // CANCELLATION APPROVAL (CALL SSL API)
        // =============================
        [HttpPost]
        public async Task<IActionResult> ApproveRefund(int bookingId)
        {
            var booking = _context.Bookings
                .Include(b => b.User)
                .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            // 🔥 CALL SSL API
            var refundSuccess = await ProcessSslRefund(booking);

            if (refundSuccess)
            {
                booking.RefundStatus = "Success";
                booking.IsCancelled = true;
                booking.RefundProcessedAt = DateTime.Now;

                await SendRefundEmail(booking);

                await _context.SaveChangesAsync();
            }
            else
            {
                booking.RefundStatus = "Failed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("BookingHistory");
        }

        // =============================
        // PROCESS REFUND WITH SSL API
        // =============================
        private async Task<bool> ProcessSslRefund(Booking booking)
        {
            var baseUrl = "https://sandbox.sslcommerz.com/validator/api/merchantTransIDvalidationAPI.php";

            var query = new Dictionary<string, string>
            {
                { "store_id", "perso69f6349142741" },
                { "store_passwd", "perso69f6349142741@ssl" },
                { "bank_tran_id", booking.BankTranId },
                { "refund_trans_id", Guid.NewGuid().ToString() },
                { "refund_amount", booking.RefundAmount.GetValueOrDefault().ToString("F2") },
                { "refund_remarks", "Booking Cancel Refund" },
                { "format", "json" }
            };

            // 🔥 BUILD QUERY STRING
            var url = baseUrl + "?" + string.Join("&",
                query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            var client = new HttpClient();

            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            // 🔥 DEBUG (VERY IMPORTANT)
            Console.WriteLine("SSL REFUND RESPONSE:");
            Console.WriteLine(result);

            // 🔥 SAFE PARSE
            SslRefundResponse json = null;

            try
            {
                json = System.Text.Json.JsonSerializer.Deserialize<SslRefundResponse>(result);
            }
            catch
            {
                Console.WriteLine("❌ Not JSON → likely API error");
                return false;
            }

            if (json == null)
                return false;

            if (json.APIConnect != "DONE")
                return false;

            if (json.status == "success" || json.status == "processing")
                return true;

            Console.WriteLine("Refund Failed Reason: " + json.errorReason);

            return false;
        }

        // =============================
        // SEND REFUND EMAIL TO USER
        // =============================
        private async Task SendRefundEmail(Booking booking)
        {
            var subject = "Refund Approved";

            var message = $@"
                Your booking #{booking.BookingId} has been cancelled.

                Refund Amount: ৳ {booking.RefundAmount}

                Status: SUCCESS
            ";

            string fromEmail = "shahriarimran2002@gmail.com";          // 🔥 your email
            string appPassword = "uxanihdkpniphluk";

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mail.To.Add(booking.User.Email);

            await smtp.SendMailAsync(mail);
        }



        // =============================
        // REFUND REJECTION (MARK AS FAILED)
        // =============================
        [HttpPost]
        public async Task<IActionResult> RejectRefund(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);

            if (booking == null)
                return NotFound();

            booking.RefundStatus = "Failed";

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageBookings");
        }

        // =============================
        // REVENUE REPORT GENERATOR
        // =============================
        [AllowAnonymous]
        public IActionResult RevenueReport(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? DateTime.Now.AddMonths(-1);
            DateTime end = endDate.HasValue ? endDate.Value.AddDays(1).AddSeconds(-1) : DateTime.Now;

            var query = _context.Bookings
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.Slot)
                        .ThenInclude(s => s.ParkingArea)
                .Include(b => b.User)
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end);

            var bookingsInRange = query.ToList();

            // Total revenue = Sum of TotalAmount - Sum of strict RefundAmount (only for successful/processed refunds)
            decimal grossRevenue = bookingsInRange.Sum(b => b.TotalAmount);
            decimal totalRefunds = bookingsInRange
                .Where(b => b.RefundStatus == "Success" || (b.IsCancelled && b.RefundAmount > 0))
                .Sum(b => b.RefundAmount ?? b.RefundPreview ?? 0);
            
            decimal netRevenue = grossRevenue - totalRefunds;

            // Separate arrays for report visibility
            var successfulBookings = bookingsInRange.Where(b => !b.IsCancelled).ToList();
            var cancelledBookings = bookingsInRange.Where(b => b.IsCancelled).ToList();

            var model = new RevenueReportViewModel
            {
                PeriodType = "Custom Date-Range",
                DateRange = $"{start:MMM dd, yyyy} - {end:MMM dd, yyyy}",
                TotalRevenue = netRevenue,
                TotalCompletedBookings = successfulBookings.Count,
                SuccessfulBookings = successfulBookings,
                CancelledBookings = cancelledBookings,
                AreaRevenues = new List<AreaRevenue>()
            };

            // Dynamically share revenue per-slot mapped back to the Block ID
            var areaGroupings = successfulBookings
                .SelectMany(b => b.BookingSlots.Select(bs => new { 
                    BlockNumber = bs.Slot.ParkingArea.BlockNumber, 
                    VehicleType = bs.Slot.ParkingArea.VehicleType,
                    RevenuveShare = b.TotalAmount / b.BookingSlots.Count
                }))
                .GroupBy(x => new { x.BlockNumber, x.VehicleType })
                .AsEnumerable() // Pull into memory to allow enum .ToString() evaluation
                .Select(g => new AreaRevenue
                {
                    BlockNumber = g.Key.BlockNumber,
                    VehicleType = g.Key.VehicleType.ToString(), // cast enum to string securely
                    Revenue = g.Sum(x => x.RevenuveShare),
                    CompletedBookings = g.Count() // Metric is total slots booked in this area
                })
                .OrderByDescending(a => a.Revenue)
                .ToList();

            model.AreaRevenues = areaGroupings;

            // Use Layout = null inside the view to keep it clean for PDF
            return View(model);
        }

    }


}

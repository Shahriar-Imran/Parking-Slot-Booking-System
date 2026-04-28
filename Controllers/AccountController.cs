using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Models;
using ParkingSystem.ViewModels;
using System.Threading.Tasks;
using ParkingSystem.Services;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using Microsoft.AspNetCore.Authorization;

namespace ParkingSystem.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _context = context;
        }

        // =========================
        // REGISTER PAGE
        // =========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =========================
        // REGISTER LOGIC
        // =========================
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FullName = model.FullName,
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 🔐 Generate token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, token = token },
                        Request.Scheme
                    );

                    // 📧 SEND EMAIL HERE ✅
                    var emailSender = new EmailSender();

                    await emailSender.SendEmailAsync(
                        user.Email,
                        "Confirm your email",
                        $"Please confirm your account by clicking here: <a href='{confirmationLink}'>Click here</a>"
                    );

                    return View("RegistrationSuccessful");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // =========================
        // CONFIRM EMAIL
        // =========================
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("Error");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return View("Error");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return View("ConfirmEmail");

            return View("Error");
        }

        // =========================
        // LOGIN PAGE
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN LOGIC
        // =========================
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    false
                );

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }

        // ================= PROFILE =================

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // ================= EDIT PROFILE =================

        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ApplicationUser model, IFormFile imageFile, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            // IMAGE UPLOAD
            if (imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "images/users");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                user.ProfileImage = "/images/users/" + fileName;
            }

            // UPDATE USER
            await _userManager.UpdateAsync(user);

            // PASSWORD UPDATE
            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, newPassword);
            }

            return RedirectToAction("Profile");
        }



        // Calculate Refund Amount
        private decimal CalculateRefund(decimal totalAmount, DateTime startTime)
        {
            var now = DateTime.Now;
            var diff = startTime - now;

            if (diff.TotalHours >= 24)
                return totalAmount * 0.90m;

            if (diff.TotalHours >= 12)
                return totalAmount * 0.75m;

            if (diff.TotalHours >= 6)
                return totalAmount * 0.50m;

            if (diff.TotalHours >= 1)
                return totalAmount * 0.25m;

            return 0; // no refund
        }

        // ================= USER BOOKING HISTORY =================
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var bookings = _context.Bookings
                .Include(b => b.BookingSlots)
                    .ThenInclude(bs => bs.Slot)
                        .ThenInclude(s => s.ParkingArea)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.StartTime)
                .ToList();

            foreach (var b in bookings)
            {
                // 🔥 Only calculate once
                if (b.RefundPreview == null)
                {
                    b.RefundPreview = CalculateRefund(b.TotalAmount, b.StartTime);
                }
            }

            await _context.SaveChangesAsync(); // 🔥 save fixed value

            return View(bookings);
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Checkout(string slots, string duration, DateTime date)
    {
        ViewBag.Date = date;
        Console.WriteLine("Slots: " + slots);
        Console.WriteLine("Duration: " + duration);

        if (string.IsNullOrEmpty(slots) || string.IsNullOrEmpty(duration))
        {
            return Content("Duration or slots missing!");
        }

        int durationInt = int.Parse(duration);

        var slotIds = slots.Split(',').Select(int.Parse).ToList();

        var selectedSlots = _context.ParkingSlots
            .Include(s => s.ParkingArea)
            .Where(s => slotIds.Contains(s.SlotId))
            .ToList();

        decimal total = selectedSlots.Sum(s => s.ParkingArea.HourlyRate * durationInt);

        ViewBag.Total = total;
        ViewBag.Duration = durationInt;
        

        return View(selectedSlots);
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

    private async Task SendCancellationEmail(Booking booking)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == booking.UserId);

        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        string fromEmail = "shahriarimran2002@gmail.com";          // 🔥 your email
        string appPassword = "nhnjcthqsdgmqlqv";           // 🔥 Gmail App Password

        string subject = "Booking Cancelled - Refund Info";

        string body = $@"
        <h3>Booking Cancelled</h3>
        <p>Booking ID: <b>{booking.BookingId}</b></p>
        <p>Refund Amount: <b>৳ {booking.RefundAmount}</b></p>
        <p>Refund Number: <b>{booking.RefundPhone}</b></p>
        <p>Your refund will be processed manually within 24 hours.</p>
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

    //========= Cancel Booking ===========//

    [HttpPost]
    public async Task<IActionResult> CancelBooking(int id, string refundNumber)
    {
        var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);

        if (booking == null)
            return NotFound();

        if (booking.StartTime <= DateTime.Now)
            return Content("Cannot cancel completed booking!");

        var refund = CalculateRefund(booking.TotalAmount, booking.StartTime);

        booking.IsCancelled = true;
        booking.RefundAmount = refund;
        booking.CancelledAt = DateTime.Now;

        // 🔥 SAVE REFUND NUMBER
        booking.RefundPhone = refundNumber;

        await _context.SaveChangesAsync();

        // 🔥 OPTIONAL: SEND EMAIL
        await SendCancellationEmail(booking);

        return RedirectToAction("MyBookings", "Account");
    }
    
}
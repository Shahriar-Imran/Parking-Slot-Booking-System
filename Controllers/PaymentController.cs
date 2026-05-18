using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ParkingSystem.Data;
using ParkingSystem.Models;
using ParkingSystem.Services;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;


public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<SlotHub> _hub;
    private readonly InvoiceService _invoiceService;
    public PaymentController(ApplicationDbContext context, IHubContext<SlotHub> hub, InvoiceService invoiceService)
    {
        _context = context;
        _hub = hub;
        _invoiceService = invoiceService;
    }


    // =========================
    // SSL PAYMENT PROCESSOR
    // =========================
    [HttpPost]
    public IActionResult ProcessPayment(string slots, int duration, decimal total, DateTime date)
    {
        if(DateTime.Now > date)
        {
            ViewBag.Error = "❌ Invalid Start time. Please reselect slots.";
            return View("DateExpired");
        }
        var tranId = Guid.NewGuid().ToString();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        var slotIds = slots.Split(',').Select(int.Parse).ToList();
        var now = DateTime.Now;

        var start = date;
        var end = start.AddHours(duration);

        start = start.AddSeconds(-start.Second).AddMilliseconds(-start.Millisecond);
        end = end.AddSeconds(-end.Second).AddMilliseconds(-end.Millisecond);

        var validLock = _context.SlotLocks
            .Where(l => slotIds.Contains(l.SlotId)
                && l.UserId == userId
                && l.ExpireTime > now
                && l.StartTime < end
                && l.EndTime > start)
            .Count();

        if (validLock != slotIds.Count)
        {
            ViewBag.Error = "❌ Your slot hold expired. Please reselect slots.";
            return View("SlotExpired");
        }

        // SAVE TEMP
        _context.TempBookings.Add(new TempBooking
        {
            UserId = userId,
            Slots = slots,
            Duration = duration,
            Total = total,
            TransactionId = tranId,
            BookingDate = date
        });

        _context.SaveChanges();

        var postData = new Dictionary<string, string>
        {
            { "store_id", "perso69f6349142741" },
            { "store_passwd", "perso69f6349142741@ssl" },

            { "total_amount", total.ToString("F2") },
            { "currency", "BDT" },
            { "tran_id", tranId },

            { "success_url", "https://localhost:7114/Payment/Success" },
            { "fail_url", "https://localhost:7114/Payment/Fail" },
            { "cancel_url", "https://localhost:7114/Booking/Checkout" },

            // 🔥 REAL USER DATA (FIXED)
            { "cus_name", user?.FullName ?? "Customer" },
            { "cus_email", user?.Email ?? "test@test.com" },
            { "cus_add1", user?.Address ?? "Dhaka" },
            { "cus_phone", user?.PhoneNumber ?? "01700000000" },

            { "shipping_method", "NO" },
            { "product_name", "Parking Booking" },
            { "product_category", "Service" },
            { "product_profile", "general" }
        };

        using (var client = new HttpClient())
        {
            var response = client.PostAsync(
                "https://sandbox.sslcommerz.com/gwprocess/v4/api.php", // ✅ FIXED
                new FormUrlEncodedContent(postData)
            ).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            dynamic json = JsonConvert.DeserializeObject(result);

            string gatewayUrl = json.GatewayPageURL;

            return Redirect(gatewayUrl);
        }
    }

    // =========================
    // SSL RESPONSE HANDLER (SUCCESS)
    // =========================
    [HttpPost]
    public async Task<IActionResult> Success()
    {
        // 🔥 READ SSL RESPONSE (POST)
        var form = Request.Form;

        string tranId = form["tran_id"];
        string bankTranId = form["bank_tran_id"];
        string status = form["status"];

        // 🔁 FALLBACK (if SSL sends GET - rare but safe)
        if (string.IsNullOrEmpty(tranId))
            tranId = Request.Query["tran_id"];

        if (string.IsNullOrEmpty(tranId))
            return Content("Invalid transaction!");

        // ❌ PAYMENT FAILED
        if (status != "VALID" && status != "VALIDATED")
        {
            return Content("❌ Payment validation failed!");
        }

        var temp = _context.TempBookings
            .FirstOrDefault(t => t.TransactionId == tranId);

        if (temp == null)
            return Content("Transaction not found!");

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            var start = temp.BookingDate;
            var end = start.AddHours(temp.Duration);

            start = start.AddSeconds(-start.Second).AddMilliseconds(-start.Millisecond);
            end = end.AddSeconds(-end.Second).AddMilliseconds(-end.Millisecond);

            var slotIds = temp.Slots.Split(',').Select(int.Parse).ToList();

            // 🔴 HARD CHECK
            var conflictExists = _context.BookingSlots
                .Any(b => slotIds.Contains(b.SlotId)
                       && !b.Booking.IsCancelled
                       && b.Booking.StartTime < end
                       && b.Booking.EndTime > start);

            if (conflictExists)
            {
                ViewBag.Error = "❌ Slot already booked. Payment failed.";
                return View("SlotBooked");
            }

            // ✅ CREATE BOOKING (🔥 SAVE bank_tran_id)
            var booking = new Booking
            {
                UserId = temp.UserId,
                StartTime = start,
                EndTime = end,
                TotalAmount = temp.Total,
                TransactionId = tranId,
                BankTranId = bankTranId   // 🔥 CRITICAL FIX
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // 🔹 SAVE SLOTS
            foreach (var slotId in slotIds)
            {
                _context.BookingSlots.Add(new BookingSlot
                {
                    BookingId = booking.BookingId,
                    SlotId = slotId
                });
            }

            await _context.SaveChangesAsync();

            // 🔥 REMOVE SLOT LOCKS + SIGNALR
            var locks = _context.SlotLocks
                .Where(l => slotIds.Contains(l.SlotId))
                .ToList();

            foreach (var lockItem in locks)
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

            _context.SlotLocks.RemoveRange(locks);
            await _context.SaveChangesAsync();

            // 🔥 DELETE TEMP
            _context.TempBookings.Remove(temp);
            await _context.SaveChangesAsync();

            // 🔥 LOAD NAVIGATION
            _context.Entry(booking)
                .Collection(b => b.BookingSlots)
                .Query()
                .Include(bs => bs.Slot)
                .ThenInclude(s => s.ParkingArea)
                .Load();

            await transaction.CommitAsync();

            // 🔥 EMAIL + PDF
            var user = _context.Users.FirstOrDefault(u => u.Id == booking.UserId);

            var qrBytes = GenerateQr(booking, user);
            var pdfBytes = _invoiceService.GenerateInvoice(booking, user, qrBytes);

            await SendInvoiceEmailWithPdf(user.Email, pdfBytes, booking.BookingId);

            return RedirectToAction("Confirmation", new { id = booking.BookingId });
        }
    }

    // =========================
    // PAYMENT FAILED
    // =========================
    public async Task<IActionResult> Fail()
    {
        ViewBag.Error = "❌ Payment Failed";
        return View("PaymentFailed");
    }

    // =========================
    // PAYMENT CANCELLED (BY SSL)
    // =========================
    public async Task<IActionResult> Cancel(string userId)
    {
        var locks = _context.SlotLocks
            .Where(l => l.UserId == userId)
            .ToList(); // 🔥 important

        // 🔥 LOOP THROUGH EACH LOCK
        foreach (var lockItem in locks)
        {
            await _hub.Clients.All.SendAsync("ReceiveSlotUpdate", new
            {
                slotId = lockItem.SlotId, // ✅ FIXED
                status = "available",
                userId = lockItem.UserId
            });
        }

        _context.SlotLocks.RemoveRange(locks);
        await _context.SaveChangesAsync();

        return RedirectToAction("Availability", "Slot");
    }

    // =========================
    // CONFIRMATION PAGE
    // =========================
    public IActionResult Confirmation(int id)
    {
        var booking = _context.Bookings
            .Include(b => b.BookingSlots)
                .ThenInclude(bs => bs.Slot)
                    .ThenInclude(s => s.ParkingArea)
            .FirstOrDefault(b => b.BookingId == id);

        if (booking == null)
            return NotFound();

        var user = _context.Users.FirstOrDefault(u => u.Id == booking.UserId);

        // 👉 No need for separate slot query anymore
        var slots = booking.BookingSlots.Select(bs => bs.Slot).ToList();

        // QR generation (keep your existing code)
        var qrBytes = GenerateQr(booking, user);
        ViewBag.QRCode = Convert.ToBase64String(qrBytes);

        ViewBag.User = user;
        ViewBag.Slots = slots;

        return View(booking);
    }

    // =========================
    // EMAIL SENDER WITH PDF ATTACHMENT
    // =========================
    private async Task SendInvoiceEmailWithPdf(string email, byte[] pdfBytes, int bookingId)
    {
        string fromEmail = "shahriarimran2002@gmail.com";
        string appPassword = "uxanihdkpniphluk";

        var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(fromEmail, appPassword),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = $"Invoice #{bookingId}",
            Body = "Your booking invoice is attached.",
            IsBodyHtml = true
        };

        mail.To.Add(email);

        // 🔥 ATTACH PDF
        mail.Attachments.Add(new Attachment(
            new MemoryStream(pdfBytes),
            $"Invoice_{bookingId}.pdf",
            "application/pdf"));

        await smtp.SendMailAsync(mail);
    }


    // =========================
    // QR GENERATION HELPER METHOD
    // =========================
    private byte[] GenerateQr(Booking booking, ApplicationUser user)
    {
        var slots = booking.BookingSlots.Select(bs => bs.Slot).ToList();

        string qrText = $"Booking ID: {booking.BookingId}\n" +
                        $"Transaction: {booking.TransactionId}\n" +
                        $"User: {user?.FullName}\n" +
                        $"Slots: {string.Join(", ", slots.Select(s => $"{s.SlotNumber} ({s.ParkingArea.VehicleType})"))}\n" +
                        $"Start: {booking.StartTime:dd MMM yyyy hh:mm tt}\n" +
                        $"End: {booking.EndTime:dd MMM yyyy hh:mm tt}\n";

        using (var qrGenerator = new QRCoder.QRCodeGenerator())
        {
            var qrData = qrGenerator.CreateQrCode(qrText, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrData);
            return qrCode.GetGraphic(5);
        }
    }
}
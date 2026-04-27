using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ParkingSystem.Data;
using ParkingSystem.Models;
using System.Security.Claims;
using System.Text;


public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    public PaymentController(ApplicationDbContext context)
    {
        _context = context;
    }


    // STEP 1: REDIRECT TO SSL
    [HttpPost]
    public IActionResult ProcessPayment(string slots, int duration, decimal total, DateTime date)
    {
        //Console.WriteLine("Received Date in Payment: " + date);
        var tranId = Guid.NewGuid().ToString();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var slotIds = slots.Split(',').Select(int.Parse).ToList();

        var now = DateTime.Now;


        // ✅ SAVE TEMP DATA
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

        // Save temporarily in session
        HttpContext.Session.SetString("slots", slots);
        HttpContext.Session.SetString("duration", duration.ToString());
        HttpContext.Session.SetString("total", total.ToString());

        var postData = new Dictionary<string, string>
        {
            { "store_id", "testbox" },
            { "store_passwd", "qwerty" },
            { "total_amount", total.ToString() },
            { "currency", "BDT" },
            { "tran_id", tranId },

            { "success_url", $"https://localhost:7114/Payment/Success?tran_id={tranId}" },
            { "fail_url", "https://localhost:7114/Payment/Fail" },
            { "cancel_url", "https://localhost:7114/Booking/Checkout" },

            { "cus_name", "Test User" },
            { "cus_email", "test@test.com" },
            { "cus_add1", "Dhaka" },
            { "cus_phone", "01700000000" },

            { "shipping_method", "NO" },
            { "product_name", "Parking Booking" },
            { "product_category", "Service" },
            { "product_profile", "general" }
        };

        using (var client = new HttpClient())
        {
            var response = client.PostAsync(
                "https://sandbox.sslcommerz.com/gwprocess/v4/api.php",
                new FormUrlEncodedContent(postData)
            ).Result;

            var result = response.Content.ReadAsStringAsync().Result;
            dynamic json = JsonConvert.DeserializeObject(result);

            string gatewayUrl = json.GatewayPageURL;

            return Redirect(gatewayUrl);
        }
    }

    // STEP 2: SUCCESS → SAVE BOOKING
    public async Task<IActionResult> Success(string tran_id)
    {
        var temp = _context.TempBookings
            .FirstOrDefault(t => t.TransactionId == tran_id);

        if (temp == null)
            return Content("Transaction not found!");

        var slotIds = temp.Slots.Split(',').Select(int.Parse).ToList();

        var start = temp.BookingDate;
        var end = start.AddHours(temp.Duration);

        var booking = new Booking
        {
            UserId = temp.UserId,
            StartTime = start,
            EndTime = end,
            TotalAmount = temp.Total,
            TransactionId = tran_id
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        foreach (var slotId in slotIds)
        {
            _context.BookingSlots.Add(new BookingSlot
            {
                BookingId = booking.BookingId,
                SlotId = slotId
            });
        }

        await _context.SaveChangesAsync();

        // 🔥 REMOVE SLOT LOCKS (PLACE HERE)
        var locks = _context.SlotLocks
            .Where(l => slotIds.Contains(l.SlotId));

        _context.SlotLocks.RemoveRange(locks);
        await _context.SaveChangesAsync();

        // ✅ DELETE TEMP DATA
        _context.TempBookings.Remove(temp);
        await _context.SaveChangesAsync();

        return RedirectToAction("Confirmation", new { id = booking.BookingId });
    }

    public IActionResult Fail()
    {
        return View();
    }

    public async Task<IActionResult> Cancel(string userId)
    {
        var locks = _context.SlotLocks
            .Where(l => l.UserId == userId);

        _context.SlotLocks.RemoveRange(locks);
        await _context.SaveChangesAsync();

        return RedirectToAction("Availability", "Slot");
    }

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
            byte[] qrBytes = qrCode.GetGraphic(5);

            ViewBag.QRCode = Convert.ToBase64String(qrBytes);
        }

        ViewBag.User = user;
        ViewBag.Slots = slots;

        return View(booking);
    }
}
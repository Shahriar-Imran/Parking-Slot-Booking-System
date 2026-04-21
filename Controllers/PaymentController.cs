using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var tranId = Guid.NewGuid().ToString();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            { "cancel_url", "https://localhost:7114/Payment/Cancel" },

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
            TotalAmount = temp.Total
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

        // ✅ DELETE TEMP DATA
        _context.TempBookings.Remove(temp);
        await _context.SaveChangesAsync();

        return RedirectToAction("Confirmation", new { id = booking.BookingId });
    }

    public IActionResult Fail()
    {
        return View();
    }

    public IActionResult Cancel()
    {
        return View();
    }

    public IActionResult Confirmation(int id)
    {
        var booking = _context.Bookings
            .Where(b => b.BookingId == id)
            .FirstOrDefault();

        return View(booking);
    }
}
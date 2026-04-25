using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;

namespace ParkingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingArea> ParkingAreas { get; set; }

        public DbSet<ParkingSlot> ParkingSlots { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<BookingSlot> BookingSlots { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Payment> Payments { get; set; }
    }
}
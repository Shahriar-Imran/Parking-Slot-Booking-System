using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;

namespace ParkingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingArea>()
                .HasIndex(a => new { a.BlockNumber, a.VehicleType })
                .IsUnique();
        }
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
        public DbSet<TempBooking> TempBookings { get; set; }
    }
}
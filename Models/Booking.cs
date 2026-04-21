using System.ComponentModel.DataAnnotations;
using ParkingSystem.Models;
namespace ParkingSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public string UserId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<BookingSlot> BookingSlots { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using ParkingSystem.Models;
namespace ParkingSystem.Models
{
    public class BookingSlot
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int SlotId { get; set; }
        public ParkingSlot Slot { get; set; }
    }
}
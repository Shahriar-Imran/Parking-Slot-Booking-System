using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class BookingSlot
    {
        [Key]
        public int BookingSlotId { get; set; }

        public int BookingId { get; set; }
        public int SlotId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        [ForeignKey("SlotId")]
        public ParkingSlot ParkingSlot { get; set; }
    }
}
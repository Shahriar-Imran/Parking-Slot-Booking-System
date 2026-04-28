using System.ComponentModel.DataAnnotations;
using ParkingSystem.Models;
namespace ParkingSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }


        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal TotalAmount { get; set; }
        public bool IsCancelled { get; set; } = false;

        public decimal? RefundAmount { get; set; }   // ✅ nullable
        public string? RefundPhone { get; set; }     // ✅ nullable
        public DateTime? CancelledAt { get; set; }
        public decimal? RefundPreview { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string TransactionId { get; set; }

        public ICollection<BookingSlot> BookingSlots { get; set; }
    }
}
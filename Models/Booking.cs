using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int DurationHours { get; set; }

        [Required]
        public int NumberOfSlots { get; set; }

        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<BookingSlot> BookingSlots { get; set; }

        public Payment Payment { get; set; }

        public Invoice Invoice { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class ParkingSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public string SlotNumber { get; set; }

        public decimal HourlyRate { get; set; }

        public SlotStatus Status { get; set; }

        [Required]
        public int AreaId { get; set; }

        [ForeignKey("AreaId")]
        public ParkingArea ParkingArea { get; set; }

        public ICollection<BookingSlot> BookingSlots { get; set; }
    }
}
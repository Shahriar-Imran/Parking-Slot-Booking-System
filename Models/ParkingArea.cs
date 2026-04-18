using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class ParkingArea
    {
        [Key]   // ✅ IMPORTANT
        public int AreaId { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        public ICollection<ParkingSlot>? ParkingSlots { get; set; }
    }
}
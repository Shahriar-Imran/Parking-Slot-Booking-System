using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class ParkingArea
    {
        [Key]
        public int AreaId { get; set; }

        [Required]
        public string BlockNumber { get; set; }

        [Required]
        public VehicleType VehicleType { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        public ICollection<ParkingSlot>? ParkingSlots { get; set; }
    }
}
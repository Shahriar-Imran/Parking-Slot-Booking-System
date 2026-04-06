using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class ParkingArea
    {
        [Key]
        public int AreaId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public ICollection<ParkingSlot> ParkingSlots { get; set; }
    }
}
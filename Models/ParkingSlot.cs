using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class ParkingSlot
    {
        [Key]
        public int SlotId { get; set; }
        public string SlotNumber { get; set; }

        [Required]
        public int AreaId { get; set; }

        // 🔥 IMPORTANT FIX
        [ForeignKey("AreaId")]
        public ParkingArea? ParkingArea { get; set; }
    }
}
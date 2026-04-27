using Microsoft.EntityFrameworkCore;
namespace ParkingSystem.Models

{
    [Index(nameof(SlotId), IsUnique = true)] 
    public class SlotLock
    {
        public int Id { get; set; }

        public int SlotId { get; set; }

        public string UserId { get; set; }

        public DateTime StartTime { get; set; }   // 🔥 ADD
        public DateTime EndTime { get; set; }

        public DateTime ExpireTime { get; set; }
    }
}

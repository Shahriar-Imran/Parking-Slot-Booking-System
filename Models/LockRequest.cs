namespace ParkingSystem.Models
{
    public class LockRequest
    {
        public int SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
    }
}

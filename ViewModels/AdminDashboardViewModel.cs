namespace ParkingSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalSlots { get; set; }
        public decimal TotalRevenue { get; set; }
        
        public System.Collections.Generic.List<ParkingSystem.Models.Booking> RecentBookings { get; set; } = new System.Collections.Generic.List<ParkingSystem.Models.Booking>();
    }
}

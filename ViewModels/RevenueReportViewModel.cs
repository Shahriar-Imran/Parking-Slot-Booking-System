using System.Collections.Generic;

namespace ParkingSystem.ViewModels
{
    public class RevenueReportViewModel
    {
        public string PeriodType { get; set; } 
        public string DateRange { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCompletedBookings { get; set; }
        public List<AreaRevenue> AreaRevenues { get; set; } = new List<AreaRevenue>();

        public List<Models.Booking> SuccessfulBookings { get; set; } = new List<Models.Booking>();
        public List<Models.Booking> CancelledBookings { get; set; } = new List<Models.Booking>();
    }

    public class AreaRevenue
    {
        public string BlockNumber { get; set; } 
        public string VehicleType { get; set; }
        public decimal Revenue { get; set; }
        public int CompletedBookings { get; set; }
    }
}

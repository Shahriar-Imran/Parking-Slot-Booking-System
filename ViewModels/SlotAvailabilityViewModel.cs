using System;
using System.Collections.Generic;
using ParkingSystem.Models;

namespace ParkingSystem.ViewModels
{
    public class SlotAvailabilityViewModel
    {
        public int AreaId { get; set; }
        public DateTime Date { get; set; }
        public int DurationHours { get; set; }
        public int NumberOfSlots { get; set; }

        public List<ParkingArea> Areas { get; set; }
        public List<ParkingSlot> AvailableSlots { get; set; }
        public string SelectedSlots { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
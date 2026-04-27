using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ParkingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string ProfileImage { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}